using Istatistik.Filters;
using Istatistik.Models;
using Istatistik.Services;
using System;
using System.Web;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    /// <summary>
    /// Pasaport bürosu. Havalimanı daima oturumdan alınır; istemciden gelmez.
    /// Okuma ve yazma için kullanıcının Pasaport bürosuna yetkili olması gerekir
    /// (SuperAdmin ve UnitAdmin tüm bürolara yetkilidir).
    /// </summary>
    [RoleAuthorize(AppRoles.SuperAdmin, AppRoles.UnitAdmin, AppRoles.BureauUser)]
    public class PassportController : Controller
    {
        private readonly IstatistikContext _db = new IstatistikContext();

        private PassportService CreateService()
        {
            var user = CurrentUser.FromSession(Session);
            return new PassportService(_db, user);
        }

        private ActionResult Run(Func<PassportService, object> action, bool allowGet = true)
        {
            var behavior = allowGet ? JsonRequestBehavior.AllowGet : JsonRequestBehavior.DenyGet;
            try
            {
                var data = action(CreateService());
                return Json(new { success = true, data }, behavior);
            }
            catch (UnauthorizedAccessException ex)
            {
                Response.StatusCode = 403;
                return Json(new { success = false, message = ex.Message }, behavior);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return Json(new { success = false, message = ex.Message }, behavior);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu." }, behavior);
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            var user = CurrentUser.FromSession(Session);
            if (user == null || string.IsNullOrWhiteSpace(user.Border) || !user.CanAccessBureau(BureauCodes.Pasaport))
                return new HttpStatusCodeResult(403, "Pasaport bürosu için yetkiniz yok.");

            ViewBag.Border = user.Border;
            return View();
        }

        [HttpGet]
        public ActionResult List(string type, int? year, string search)
        {
            return Run(s => s.List(type, year, search));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(string type, string payload)
        {
            return Run(s => s.Save(type, payload), false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string type, int id)
        {
            return Run(s => { s.Delete(type, id); return new { id }; }, false);
        }

        [HttpGet]
        public ActionResult MonthlyPassengerStats(int year, string flightType = null)
        {
            HatTuru? ht = string.IsNullOrWhiteSpace(flightType) ? (HatTuru?)null : PassportService.ParseHatTuru(flightType);
            return Run(s => s.GetMonthlyPassengerStats(year, ht));
        }

        [HttpGet]
        public ActionResult DailyTimeSeries(DateTime startDate, DateTime endDate)
        {
            return Run(s => s.GetDailyTimeSeries(startDate, endDate));
        }

        [HttpGet]
        public ActionResult InadSummaryByNationality()
        {
            return Run(s => s.GetInadSummaryByNationality());
        }

        [HttpGet]
        public ActionResult TahditRecords(string q)
        {
            return Run(s => s.GetTahditRecords(q));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(string type, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { success = false, message = "Dosya seçilmedi" });

            return Run(s =>
            {
                int count;
                switch ((type ?? "").ToLowerInvariant())
                {
                    case "yolcuucak": count = s.ImportYolcuUcakCsv(file.InputStream); break;
                    case "gunluk": count = s.ImportGunlukCsv(file.InputStream); break;
                    case "inad": count = s.ImportInadJson(file.InputStream); break;
                    case "tahdit": count = s.ImportTahditJson(file.InputStream); break;
                    case "haftalik": count = s.ImportHaftalikJson(file.InputStream); break;
                    default: throw new ArgumentException("Geçersiz veri tipi");
                }
                return new { count };
            }, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
