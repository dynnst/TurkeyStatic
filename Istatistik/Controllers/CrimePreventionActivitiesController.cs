using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    public class CrimePreventionActivitiesController : Controller
    {
        private static List<CrimePreventionActivity> activities = new List<CrimePreventionActivity>();

        [HttpGet]
        public ActionResult GetByDate(int unitId, DateTime date)
        {
            try
            {
                var stats = activities.Where(a => a.UnitId == unitId && a.EntryDate.Date == date.Date).ToList();
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

                model.ActivityId = activities.Count > 0 ? activities.Max(a => a.ActivityId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                activities.Add(model);

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
                var activity = activities.FirstOrDefault(a => a.ActivityId == id);
                if (activity != null)
                    activities.Remove(activity);

                return Json(new { success = true, message = "Başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
