using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    public class UnitsController : Controller
    {
        private static List<Unit> units = new List<Unit>();

        public UnitsController()
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
    }
}
