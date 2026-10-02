using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    public class CrimeStatisticsController : Controller
    {
        private static List<CrimeStatistic> crimes = new List<CrimeStatistic>();

        [HttpGet]
        public ActionResult GetByDate(int unitId, DateTime date)
        {
            try
            {
                var stats = crimes.Where(c => c.UnitId == unitId && c.EntryDate.Date == date.Date).ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(CrimeStatistic model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Geçersiz veri" });
                }

                model.CrimeStatisticId = crimes.Count > 0 ? crimes.Max(c => c.CrimeStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                crimes.Add(model);

                return Json(new { success = true, message = "Baþarýyla kaydedildi", data = model });
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
                var crime = crimes.FirstOrDefault(c => c.CrimeStatisticId == id);
                if (crime != null)
                    crimes.Remove(crime);

                return Json(new { success = true, message = "Baþarýyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
