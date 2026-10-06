using Istatistik.Filters;
using Istatistik.Models;
using Istatistik.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    /// <summary>
    /// Kullanıcının yetkili olduğu büroları listeler ve ilgili veri giriş ekranına yönlendirir.
    /// </summary>
    [RoleAuthorize(AppRoles.SuperAdmin, AppRoles.UnitAdmin, AppRoles.BureauUser)]
    public class BureauController : Controller
    {
        public class BureauItem
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
        }

        [HttpGet]
        public ActionResult Index()
        {
            var user = CurrentUser.FromSession(Session);
            ViewBag.Border = user?.Border ?? "";
            ViewBag.Role = user?.Role ?? "";

            var items = new List<BureauItem>();
            if (user == null || string.IsNullOrWhiteSpace(user.Border))
                return View(items);

            using (var db = new IstatistikContext())
            {
                BureauSeeder.EnsureForBorder(db, user.Border);

                var all = db.Bureaus
                    .Where(b => b.Border == user.Border && b.IsActive)
                    .OrderBy(b => b.Name)
                    .ToList();

                foreach (var b in all.Where(b => user.CanAccessBureau(b.Code)))
                {
                    items.Add(new BureauItem { Code = b.Code, Name = b.Name, Url = ResolveUrl(b.Code) });
                }
            }

            return View(items);
        }

        private string ResolveUrl(string code)
        {
            if (code == BureauCodes.Pasaport)
                return Url.Action("Index", "Passport");

            return Url.Action("Index", "BureauData", new { bureau = code });
        }
    }
}
