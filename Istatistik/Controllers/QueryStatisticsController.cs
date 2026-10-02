using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    public class QueryStatisticsController : Controller
    {
        private static List<QueryStatistic> queries = new List<QueryStatistic>();

        [HttpGet]
        public ActionResult GetByDate(int unitId, DateTime date)
        {
            try
            {
                var stats = queries.Where(q => q.UnitId == unitId && q.EntryDate.Date == date.Date).ToList();
                return Json(new { success = true, data = stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(QueryStatistic model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Geçersiz veri" });
                }

                model.QueryStatisticId = queries.Count > 0 ? queries.Max(q => q.QueryStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                queries.Add(model);

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
                var query = queries.FirstOrDefault(q => q.QueryStatisticId == id);
                if (query != null)
                    queries.Remove(query);

                return Json(new { success = true, message = "Baþarýyla silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
