using Istatistik.Filters;
using Istatistik.Models;
using Istatistik.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    /// <summary>
    /// Yetki yönetimi. SuperAdmin: tüm havalimanları. UnitAdmin: yalnızca kendi havalimanı.
    /// </summary>
    [RoleAuthorize(AppRoles.SuperAdmin, AppRoles.UnitAdmin)]
    public class AdminController : Controller
    {
        private readonly IstatistikContext _db = new IstatistikContext();

        private AdminService CreateService()
        {
            return new AdminService(_db, CurrentUser.FromSession(Session));
        }

        private ActionResult Run(Func<AdminService, object> action, bool allowGet = false)
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
            var cu = CurrentUser.FromSession(Session);
            ViewBag.IsSuperAdmin = cu.IsSuperAdmin;
            ViewBag.OwnBorder = cu.Border;
            return View();
        }

        [HttpGet]
        public ActionResult Borders()
        {
            return Run(s => s.GetBorders(), true);
        }

        [HttpGet]
        public ActionResult Users(string border, string search)
        {
            return Run(s =>
            {
                s.EnsureDefaultBureaus(border);
                return s.GetUsers(border, search);
            }, true);
        }

        [HttpGet]
        public ActionResult Bureaus(string border)
        {
            return Run(s =>
            {
                s.EnsureDefaultBureaus(border);
                return s.GetBureaus(border).Select(b => new { b.BureauId, b.Code, b.Name, b.IsActive }).ToList();
            }, true);
        }

        [HttpGet]
        [RoleAuthorize(AppRoles.SuperAdmin)]
        public ActionResult SuperAdmins()
        {
            return Run(s => s.GetSuperAdmins().Select(a => new { a.Sicil, a.Border, a.CreatedBy, a.CreatedDate }).ToList(), true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetRole(string sicil, string role, bool isActive = true)
        {
            return Run(s => { s.SetRole(sicil, role, null, isActive); return new { sicil, role }; });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveAssignment(string sicil)
        {
            return Run(s => { s.RemoveAssignment(sicil); return new { sicil }; });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetUserBureaus(string sicil, int[] bureauIds)
        {
            return Run(s => { s.SetUserBureaus(sicil, (bureauIds ?? new int[0]).ToList()); return new { sicil }; });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBureau(string border, string code, string name)
        {
            return Run(s =>
            {
                var b = s.AddBureau(border, code, name);
                return new { b.BureauId, b.Code, b.Name, b.IsActive };
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetBureauActive(int bureauId, bool isActive)
        {
            return Run(s => { s.SetBureauActive(bureauId, isActive); return new { bureauId, isActive }; });
        }

        // -------------------------
        // DUMMY DATA SEEDER
        // -------------------------
        [AllowAnonymous]
        public ActionResult Seed50()
        {
            var border = "ANTALYA GAZİPAŞA HAVA LİMANI";
            var rnd = new Random();
            var now = DateTime.Now;

            // Generate 50 dates over the last 2 years
            for (int i = 0; i < 50; i++)
            {
                var d = now.AddDays(-rnd.Next(1, 700));

                _db.BilgiTeknolojileriIstatistikleri.Add(new BilgiTeknolojileriIstatistik
                {
                    Border = border, Tarih = d,
                    KameraKaydiIncelemesi = rnd.Next(0, 10),
                    PtsAracAraniyor = rnd.Next(0, 5),
                    PtsAracCalinti = rnd.Next(0, 2),
                    PtsPlakaCalinti = rnd.Next(0, 2),
                    PtsPlakaKayip = rnd.Next(0, 2),
                    TahditBakilanSorunluYolcu = rnd.Next(50, 200),
                    YurdaGirisCikisBelgeTalebi = rnd.Next(0, 10),
                    TahditEkleme = rnd.Next(1, 10),
                    TahditKaldirma = rnd.Next(1, 10)
                });

                _db.CctvIstatistikleri.Add(new CctvIstatistik
                {
                    Border = border, Tarih = d, Bolge = "Terminal İçi",
                    IpSabit = rnd.Next(10, 50), IpHareketli = rnd.Next(5, 20),
                    AnalogSabit = rnd.Next(0, 10), AnalogHareketli = rnd.Next(0, 5)
                });

                _db.TrafikIstatistikleri.Add(new TrafikIstatistik
                {
                    Border = border, Tarih = d,
                    KontrolEdilenAracSayisi = rnd.Next(20, 100),
                    CezaYazilanSurucuSayisi = rnd.Next(0, 20),
                    TrafiktenMenEdilenAracSayisi = rnd.Next(0, 5),
                    GeciciGeriAlinanSurucuBelgesi = rnd.Next(0, 3)
                });

                _db.GbtUyapSorgulari.Add(new GbtUyapSorgu
                {
                    Border = border, Tarih = d,
                    SorgulananKisiSayisi = rnd.Next(100, 1000),
                    YakalananKisiSayisi = rnd.Next(0, 5)
                });

                _db.YtsSorgulari.Add(new YtsSorgu
                {
                    Border = border, Tarih = d, GunlukSorguSayisi = rnd.Next(50, 300)
                });
            }

            _db.SaveChanges();
            return Content("Başarıyla 50'şer adet rastgele veri eklendi!");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
