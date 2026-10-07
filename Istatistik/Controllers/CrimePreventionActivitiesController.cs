using Istatistik.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    public class CrimePreventionActivitiesController : Controller
    {
        private IstatistikContext _db = new IstatistikContext();

        [HttpGet]
        public ActionResult GetByDate(int unitId, DateTime date)
        {
            try
            {
                var targetDate = date.Date;
                var stats = _db.CrimePreventionActivities
                    .Where(a => a.UnitId == unitId && a.EntryDate == targetDate)
                    .ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(CrimePreventionActivity model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Geçersiz veri" });
                }

                model.CreatedDate = DateTime.Now;
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
        public ActionResult Delete(dynamic request)
        {
            try
            {
                int id = request.id;
                var activity = _db.CrimePreventionActivities.FirstOrDefault(a => a.ActivityId == id);
                if (activity != null)
                {
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
