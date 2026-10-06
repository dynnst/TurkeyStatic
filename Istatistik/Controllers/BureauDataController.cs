using Istatistik.Filters;
using Istatistik.Models;
using Istatistik.Services;
using System;
using System.Web.Mvc;

namespace Istatistik.Controllers
{
    [RoleAuthorize(AppRoles.SuperAdmin, AppRoles.UnitAdmin, AppRoles.BureauUser)]
    public class BureauDataController : Controller
    {
        private DataService GetService(string bureauCode)
        {
            var user = CurrentUser.FromSession(Session);
            if (user == null || string.IsNullOrWhiteSpace(user.Border))
                throw new Exception("Kullanıcı oturumu geçersiz.");

            if (!user.CanAccessBureau(bureauCode))
                throw new Exception("Bu büro için yetkiniz yok.");

            return new DataService(user.Border);
        }

        public ActionResult Index(string bureau)
        {
            var user = CurrentUser.FromSession(Session);
            if (user == null || string.IsNullOrWhiteSpace(user.Border) || !user.CanAccessBureau(bureau))
                return RedirectToAction("Index", "Bureau");

            ViewBag.Bureau = bureau;
            ViewBag.Border = user.Border;
            ViewBag.Role = user.Role;
            ViewBag.UserName = user.Sicil;

            return View();
        }

        [HttpGet]
        public JsonResult Get(string bureau, string type, int? id, int? year, int? month)
        {
            try
            {
                var s = GetService(bureau);
                var data = s.Get(type, id, year, month);
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Save(string bureau, string type, string payload)
        {
            try
            {
                var s = GetService(bureau);
                var res = s.Save(type, payload);
                return Json(new { success = true, data = res });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(string bureau, string type, int id)
        {
            try
            {
                var s = GetService(bureau);
                s.Delete(type, id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
