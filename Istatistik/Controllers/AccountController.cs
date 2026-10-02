using Istatistik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;

namespace Istatistik.Controllers
{
    public class AccountController : Controller
    {
        // In-memory user storage (ilgili sistemde veritabaný kullanýlacak)
        private static List<User> users = new List<User>();

        public AccountController()
        {
            // Ýlk baþlatmada örnek kullanýcýlar ekle
            if (users.Count == 0)
            {
                users.Add(new User
                {
                    UserId = 1,
                    Username = "admin",
                    Email = "admin@example.com",
                    FullName = "Sistem Yöneticisi",
                    PasswordHash = HashPassword("admin123"),
                    UnitId = 1,
                    Role = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });

                users.Add(new User
                {
                    UserId = 2,
                    Username = "user",
                    Email = "user@example.com",
                    FullName = "Veri Giriþ Personeli",
                    PasswordHash = HashPassword("user123"),
                    UnitId = 1,
                    Role = "DataEntryPersonnel",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
            }
        }

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
                ViewBag.ErrorMessage = "Kullanýcý adý ve þifre gereklidir!";
                return View();
            }

            var user = users.FirstOrDefault(u =>
                u.Username == username &&
                u.IsActive &&
                VerifyPassword(password, u.PasswordHash));

            if (user == null)
            {
                ViewBag.ErrorMessage = "Kullanýcý adý veya þifre hatalý!";
                return View();
            }

            // Kullanýcý oturum bilgisini güncelle
            user.LastLoginDate = DateTime.Now;

            // Forms Authentication ile giriþ yap
            FormsAuthentication.SetAuthCookie(username, false);

            // Session'a kullanýcý bilgilerini kaydet
            Session["UserId"] = user.UserId;
            Session["Username"] = user.Username;
            Session["FullName"] = user.FullName;
            Session["UserRole"] = user.Role;
            Session["UnitId"] = user.UnitId;

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.Units = GetUnits();
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string username, string email, string fullname, string password, string confirmPassword, int unitId)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Tüm alanlar gereklidir!";
                ViewBag.Units = GetUnits();
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Þifreler eþleþmiyor!";
                ViewBag.Units = GetUnits();
                return View();
            }

            if (users.Any(u => u.Username == username))
            {
                ViewBag.ErrorMessage = "Bu kullanýcý adý zaten kullanýlýyor!";
                ViewBag.Units = GetUnits();
                return View();
            }

            var newUser = new User
            {
                UserId = users.Count > 0 ? users.Max(u => u.UserId) + 1 : 1,
                Username = username,
                Email = email,
                FullName = fullname,
                PasswordHash = HashPassword(password),
                UnitId = unitId,
                Role = "DataEntryPersonnel",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            users.Add(newUser);

            ViewBag.SuccessMessage = "Kayýt baþarýlý! Giriþ yapýnýz.";
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
            var userId = (int?)Session["UserId"];
            if (userId == null)
                return RedirectToAction("Login");

            var user = users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        // Helper Methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashOfInput = HashPassword(password);
                return hashOfInput.Equals(hash);
            }
            catch
            {
                return false;
            }
        }

        private List<Unit> GetUnits()
        {
            var units = new List<Unit>
            {
                new Unit { UnitId = 1, UnitName = "Suç Önleme", Description = "Suç Önleme Þubesi", IsActive = true },
                new Unit { UnitId = 2, UnitName = "Ýdari Büro", Description = "Ýdari Ýþler Bürosu", IsActive = true },
                new Unit { UnitId = 3, UnitName = "Pasaport Büro", Description = "Pasaport Ýþleri Bürosu", IsActive = true },
                new Unit { UnitId = 4, UnitName = "Trafik", Description = "Trafik Þubesi", IsActive = true }
            };
            return units;
        }
    }
}
