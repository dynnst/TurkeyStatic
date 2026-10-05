using Istatistik.Models;
using Istatistik.Services;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace Istatistik.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Kullanıcı adı ve şifre gereklidir!";
                return View();
            }

            var user = AuthService.Authenticate(username, password);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı ya da sistemde yetkiniz yok!";
                return View();
            }

            FormsAuthentication.SetAuthCookie(user.Sicil, false);

            Session["UserId"] = user.UserId;
            Session["Username"] = user.Sicil;
            Session["FullName"] = user.FullName;
            Session["UserRole"] = user.Role;
            Session["Border"] = user.Border;
            Session["BureauCodes"] = user.BureauCodes;
            Session["LoginTime"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            return RedirectToAction("Index", "Home");
        }

        // Kayıt, yetkilendirme birim yöneticileri tarafından yapıldığı için kapalıdır.
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return RedirectToAction("Login");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Profile()
        {
            var cu = CurrentUser.FromSession(Session);
            if (cu == null)
                return RedirectToAction("Login");

            var model = new User
            {
                UserId = (Session["UserId"] as int?) ?? 0,
                Username = cu.Sicil,
                FullName = Session["FullName"] as string,
                Role = cu.Role,
                IsActive = true
            };
            return View(model);
        }
    }
}
