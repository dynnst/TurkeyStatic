using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

        public ActionResult Index()
        {
            return View();
        }

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

        // API Methods for Crime Statistics
        [HttpGet]
        public ActionResult GetCrimeStatistics(int unitId, DateTime date)
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
        public ActionResult SaveCrimeStatistic(CrimeStatistic model)
        {
            try
            {
                model.CrimeStatisticId = crimes.Count > 0 ? crimes.Max(c => c.CrimeStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                crimes.Add(model);

                return Json(new { success = true, message = "Kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteCrimeStatistic(int id)
        {
            try
            {
                var crime = crimes.FirstOrDefault(c => c.CrimeStatisticId == id);
                if (crime != null)
                    crimes.Remove(crime);

                return Json(new { success = true, message = "Silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API Methods for Query Statistics
        [HttpGet]
        public ActionResult GetQueryStatistics(int unitId, DateTime date)
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
        public ActionResult SaveQueryStatistic(QueryStatistic model)
        {
            try
            {
                model.QueryStatisticId = queries.Count > 0 ? queries.Max(q => q.QueryStatisticId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                queries.Add(model);

                return Json(new { success = true, message = "Kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteQueryStatistic(int id)
        {
            try
            {
                var query = queries.FirstOrDefault(q => q.QueryStatisticId == id);
                if (query != null)
                    queries.Remove(query);

                return Json(new { success = true, message = "Silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API Methods for Crime Prevention Activities
        [HttpGet]
        public ActionResult GetActivities(int unitId, DateTime date)
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
        public ActionResult SaveActivity(CrimePreventionActivity model)
        {
            try
            {
                model.ActivityId = activities.Count > 0 ? activities.Max(a => a.ActivityId) + 1 : 1;
                model.CreatedDate = DateTime.Now;
                activities.Add(model);

                return Json(new { success = true, message = "Kaydedildi", data = model });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteActivity(int id)
        {
            try
            {
                var activity = activities.FirstOrDefault(a => a.ActivityId == id);
                if (activity != null)
                    activities.Remove(activity);

                return Json(new { success = true, message = "Silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}