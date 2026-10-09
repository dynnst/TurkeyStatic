using Istatistik.Filters;
using Istatistik.Models;
using Istatistik.Services;
using System;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    /// <summary>
    /// Raporlama ve görselleştirme. Havalimanı daima oturumdan alınır; istemciden gelmez.
    /// </summary>
    [RoleAuthorize(AppRoles.SuperAdmin, AppRoles.UnitAdmin, AppRoles.BureauUser)]
    public class ReportsController : Controller
    {
        private readonly IstatistikContext _db = new IstatistikContext();

        private ReportService CreateService()
        {
            var user = CurrentUser.FromSession(Session);
            return new ReportService(_db, user);
        }

        private ActionResult Run(Func<ReportService, object> action, bool allowGet = true)
        {
            var behavior = allowGet ? JsonRequestBehavior.AllowGet : JsonRequestBehavior.DenyGet;
            try
            {
                var data = action(CreateService());
                return Json(new { success = true, data = data }, behavior);
            }
            catch (UnauthorizedAccessException ex)
            {
                Response.StatusCode = 403;
                return Json(new { success = false, message = ex.Message }, behavior);
            }
            catch (ArgumentException ex)
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = ex.Message }, behavior);
            }
            catch (InvalidOperationException ex)
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = ex.Message }, behavior);
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." }, behavior);
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            var user = CurrentUser.FromSession(Session);
            if (user == null || string.IsNullOrWhiteSpace(user.Border))
                return RedirectToAction("Login", "Account");

            ViewBag.Border = user.Border;
            ViewBag.UserRole = user.Role;
            ViewBag.CurrentUser = user;
            return View();
        }

        [HttpGet]
        public ActionResult Compare()
        {
            var user = CurrentUser.FromSession(Session);
            if (user == null || string.IsNullOrWhiteSpace(user.Border))
                return RedirectToAction("Login", "Account");

            ViewBag.Border = user.Border;
            ViewBag.UserRole = user.Role;
            ViewBag.CurrentUser = user;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetChartData(string dataType, DateTime startDate, DateTime endDate, string periodType)
        {
            var period = ParsePeriodType(periodType);
            return Run(s => s.GetAggregatedData(dataType, startDate, endDate, period), false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetTableData(string dataType, DateTime startDate, DateTime endDate, string periodType, int page = 1, int pageSize = 25, string sortBy = null, bool sortDesc = false)
        {
            var period = ParsePeriodType(periodType);
            return Run(s => s.GetPagedTableData(dataType, startDate, endDate, period, page, pageSize, sortBy, sortDesc), false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetComparisonData(string dataType, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End, string periodType)
        {
            var period = ParsePeriodType(periodType);
            return Run(s => s.GetComparisonData(dataType, period1Start, period1End, period2Start, period2End, period), false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportCsv(string dataType, DateTime startDate, DateTime endDate, string periodType)
        {
            try
            {
                var service = CreateService();
                var period = ParsePeriodType(periodType);
                var data = service.GetPagedTableData(dataType, startDate, endDate, period, 1, int.MaxValue, null, false);
                var bytes = service.ExportToCsv(data, "rapor.csv");
                return File(bytes, "text/csv", string.Format("rapor_{0}_{1}.csv", dataType, DateTime.Now.ToString("yyyyMMdd")));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportExcel(string dataType, DateTime startDate, DateTime endDate, string periodType)
        {
            try
            {
                var service = CreateService();
                var period = ParsePeriodType(periodType);
                var data = service.GetPagedTableData(dataType, startDate, endDate, period, 1, int.MaxValue, null, false);
                var bytes = service.ExportToExcel(data, "rapor.xlsx");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("rapor_{0}_{1}.xlsx", dataType, DateTime.Now.ToString("yyyyMMdd")));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Grafik görseli (base64) + veri tablosunu birlikte Excel'e aktarır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportChartExcel(string dataType, DateTime startDate, DateTime endDate, string periodType, string chartImageBase64, string chartTitle)
        {
            try
            {
                var service = CreateService();
                var period = ParsePeriodType(periodType);
                var aggregated = service.GetAggregatedData(dataType, startDate, endDate, period);
                var bytes = service.ExportToExcelWithChart(aggregated, chartImageBase64, chartTitle ?? dataType);
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    string.Format("grafik_{0}_{1}.xlsx", dataType, DateTime.Now.ToString("yyyyMMdd")));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportComparisonExcel(string dataType, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End, string periodType, string chartImageBase64)
        {
            try
            {
                var service = CreateService();
                var period = ParsePeriodType(periodType);
                var compResult = service.GetComparisonData(dataType, period1Start, period1End, period2Start, period2End, period);
                var bytes = service.ExportComparisonExcel(compResult, chartImageBase64);
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    string.Format("karsilastirma_{0}_{1}.xlsx", dataType, DateTime.Now.ToString("yyyyMMdd")));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        private PeriodType ParsePeriodType(string periodType)
        {
            switch ((periodType ?? "").ToLower())
            {
                case "daily":
                case "günlük":
                    return PeriodType.Daily;
                case "weekly":
                case "haftalık":
                    return PeriodType.Weekly;
                case "yearly":
                case "yıllık":
                    return PeriodType.Yearly;
                default:
                    return PeriodType.Monthly;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
