using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Istatistik.Models;

namespace Istatistik.Controllers
{
    public class HomeController : Controller
    {
        // In-memory storage (ilgili session'a kaydedilir)
        private static List<Unit> units = new List<Unit>();
        private static List<CrimeStatistic> crimes = new List<CrimeStatistic>();
        private static List<QueryStatistic> queries = new List<QueryStatistic>();
        private static List<CrimePreventionActivity> activities = new List<CrimePreventionActivity>();

        public HomeController()
        {
            // İlk başlatmada birimler ekle
            if (units.Count == 0)
            {
                units.Add(new Unit { UnitId = 1, UnitName = "Suç Önleme", Description = "Suç Önleme Şubesi", IsActive = true });
                units.Add(new Unit { UnitId = 2, UnitName = "İdari Büro", Description = "İdari İşler Bürosu", IsActive = true });
                units.Add(new Unit { UnitId = 3, UnitName = "Pasaport Büro", Description = "Pasaport İşleri Bürosu", IsActive = true });
                units.Add(new Unit { UnitId = 4, UnitName = "Trafik", Description = "Trafik Şubesi", IsActive = true });
            }
        }

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

        // API Methods for Units
        [HttpGet]
        public ActionResult GetUnits()
        {
            try
            {
                var activeUnits = units.Where(u => u.IsActive).OrderBy(u => u.UnitName).ToList();
                return Json(new { success = true, data = activeUnits }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetAll()
        {
            try
            {
                var activeUnits = units.Where(u => u.IsActive).OrderBy(u => u.UnitName).ToList();
                return Json(new { success = true, data = activeUnits }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method - Kullanıcının birim yetkisini kontrol et
        private bool VerifyUserUnitAccess(int requestedUnitId)
        {
            var userUnitId = Session["UnitId"] as int?;
            var userRole = Session["UserRole"] as string;

            // Admin'ler tüm birimlere erişebilir
            if (userRole == "Admin")
                return true;

            // Diğer kullanıcılar sadece kendi birimine erişebilir
            return userUnitId == requestedUnitId;
        }

        // API Methods for Crime Statistics
        [HttpGet]
        [Authorize]
        public ActionResult GetCrimeStatistics(int unitId, DateTime date)
        {
            try
            {
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(unitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);
                }

                var stats = crimes.Where(c => c.UnitId == unitId && c.EntryDate.Date == date.Date).ToList();
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
            try
            {
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(unitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);
                }

                var stats = crimes.Where(c => c.UnitId == unitId && c.EntryDate.Date == date.Date).ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveCrimeStatistic(CrimeStatistic model)
        {
            try
            {
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(model.UnitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                }

                model.CrimeStatisticId = crimes.Count > 0 ? crimes.Max(c => c.CrimeStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                crimes.Add(model);

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
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Geçersiz veri" });
                }

                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(model.UnitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                }

                model.CrimeStatisticId = crimes.Count > 0 ? crimes.Max(c => c.CrimeStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                crimes.Add(model);

                return Json(new { success = true, message = "Başarıyla kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteCrimeStatistic(int id)
        {
            try
            {
                var crime = crimes.FirstOrDefault(c => c.CrimeStatisticId == id);
                if (crime != null)
                {
                    // Birim yetkisini kontrol et
                    if (!VerifyUserUnitAccess(crime.UnitId))
                    {
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                    }

                    crimes.Remove(crime);
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
            try
            {
                int id = request.id;
                var crime = crimes.FirstOrDefault(c => c.CrimeStatisticId == id);
                if (crime != null)
                {
                    // Birim yetkisini kontrol et
                    if (!VerifyUserUnitAccess(crime.UnitId))
                    {
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                    }

                    crimes.Remove(crime);
                }

                return Json(new { success = true, message = "Başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API Methods for Query Statistics
        [HttpGet]
        [Authorize]
        public ActionResult GetQueryStatistics(int unitId, DateTime date)
        {
            try
            {
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(unitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);
                }

                var stats = queries.Where(q => q.UnitId == unitId && q.EntryDate.Date == date.Date).ToList();
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
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(model.UnitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                }

                model.QueryStatisticId = queries.Count > 0 ? queries.Max(q => q.QueryStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                queries.Add(model);

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
                var query = queries.FirstOrDefault(q => q.QueryStatisticId == id);
                if (query != null)
                {
                    // Birim yetkisini kontrol et
                    if (!VerifyUserUnitAccess(query.UnitId))
                    {
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                    }

                    queries.Remove(query);
                }

                return Json(new { success = true, message = "Başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API Methods for Crime Prevention Activities
        [HttpGet]
        [Authorize]
        public ActionResult GetActivities(int unitId, DateTime date)
        {
            try
            {
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(unitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" }, JsonRequestBehavior.AllowGet);
                }

                var stats = activities.Where(a => a.UnitId == unitId && a.EntryDate.Date == date.Date).ToList();
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
                // Birim yetkisini kontrol et
                if (!VerifyUserUnitAccess(model.UnitId))
                {
                    return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                }

                model.ActivityId = activities.Count > 0 ? activities.Max(a => a.ActivityId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = Session["Username"] as string;
                activities.Add(model);

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
                var activity = activities.FirstOrDefault(a => a.ActivityId == id);
                if (activity != null)
                {
                    // Birim yetkisini kontrol et
                    if (!VerifyUserUnitAccess(activity.UnitId))
                    {
                        return Json(new { success = false, message = "Bu birime erişim yetkiniz yok!" });
                    }

                    activities.Remove(activity);
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
    }
}