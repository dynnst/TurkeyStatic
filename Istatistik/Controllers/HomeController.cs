using Istatistik.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using Istatistik.Services;

namespace Istatistik.Controllers
{
    public class HomeController : Controller
    {
        private IstatistikContext _db = new IstatistikContext();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult DataEntry()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetUnits()
        {
            try
            {
                var activeUnits = _db.Units.ToList().Where(u => u.IsActive && VerifyUserUnitAccess(u.UnitId)).OrderBy(u => u.UnitName).ToList();
                return Json(new { success = true, data = activeUnits }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetAll()
        {
            try
            {
                var activeUnits = _db.Units.ToList().Where(u => u.IsActive && VerifyUserUnitAccess(u.UnitId)).OrderBy(u => u.UnitName).ToList();
                return Json(new { success = true, data = activeUnits }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private static readonly Dictionary<int, string> UnitBureauCodes = new Dictionary<int, string>
        {
            { 1, "SUC_ONLEME" },
            { 2, "IDARI" },
            { 3, BureauCodes.Pasaport },
            { 4, "TRAFIK" }
        };

        private bool VerifyUserUnitAccess(int requestedUnitId)
        {
            var user = CurrentUser.FromSession(Session);
            if (user == null || string.IsNullOrEmpty(user.Border))
                return false;

            string code;
            if (!UnitBureauCodes.TryGetValue(requestedUnitId, out code))
                return false;

            return user.CanAccessBureau(code);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetCrimeStatistics(int unitId, DateTime date)
        {
            try
            {
                if (!VerifyUserUnitAccess(unitId))
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);

                var targetDate = date.Date;
                var stats = _db.CrimeStatistics.Where(c => c.UnitId == unitId && c.EntryDate == targetDate).ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetByDate(int unitId, DateTime date)
        {
            return GetCrimeStatistics(unitId, date);
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveCrimeStatistic(CrimeStatistic model)
        {
            try
            {
                if (!VerifyUserUnitAccess(model.UnitId))
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });

                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                model.EntryDate = model.EntryDate.Date;
                _db.CrimeStatistics.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Başarıyla kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(CrimeStatistic model)
        {
            return SaveCrimeStatistic(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteCrimeStatistic(int id)
        {
            try
            {
                var crime = _db.CrimeStatistics.FirstOrDefault(c => c.CrimeStatisticId == id);
                if (crime != null)
                {
                    if (!VerifyUserUnitAccess(crime.UnitId))
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });

                    _db.CrimeStatistics.Remove(crime);
                    _db.SaveChanges();
                }
                return Json(new { success = true, message = "Başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Delete(dynamic request)
        {
            int id = request.id;
            return DeleteCrimeStatistic(id);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetQueryStatistics(int unitId, DateTime date)
        {
            try
            {
                if (!VerifyUserUnitAccess(unitId))
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);

                var targetDate = date.Date;
                var stats = _db.QueryStatistics.Where(q => q.UnitId == unitId && q.EntryDate == targetDate).ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveQueryStatistic(QueryStatistic model)
        {
            try
            {
                if (!VerifyUserUnitAccess(model.UnitId))
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });

                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                model.EntryDate = model.EntryDate.Date;
                _db.QueryStatistics.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Başarıyla kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteQueryStatistic(int id)
        {
            try
            {
                var query = _db.QueryStatistics.FirstOrDefault(q => q.QueryStatisticId == id);
                if (query != null)
                {
                    if (!VerifyUserUnitAccess(query.UnitId))
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });

                    _db.QueryStatistics.Remove(query);
                    _db.SaveChanges();
                }
                return Json(new { success = true, message = "Başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetActivities(int unitId, DateTime date)
        {
            try
            {
                if (!VerifyUserUnitAccess(unitId))
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);

                var targetDate = date.Date;
                var stats = _db.CrimePreventionActivities.Where(a => a.UnitId == unitId && a.EntryDate == targetDate).ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveActivity(CrimePreventionActivity model)
        {
            try
            {
                if (!VerifyUserUnitAccess(model.UnitId))
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });

                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                model.EntryDate = model.EntryDate.Date;
                _db.CrimePreventionActivities.Add(model);
                _db.SaveChanges();

                return Json(new { success = true, message = "Başarıyla kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteActivity(int id)
        {
            try
            {
                var activity = _db.CrimePreventionActivities.FirstOrDefault(a => a.ActivityId == id);
                if (activity != null)
                {
                    if (!VerifyUserUnitAccess(activity.UnitId))
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });

                    _db.CrimePreventionActivities.Remove(activity);
                    _db.SaveChanges();
                }
                return Json(new { success = true, message = "Başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Sistem Hakkında";
            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "İletişim sayfası";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
