using System;
using System.Web.Mvc;

namespace Istatistik.Filters
{
    /// <summary>
    /// Birim bazlý yetkilendirme kontrolü
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByUnitAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            // Önce standart authorization kontrolü yap
            if (!base.AuthorizeCore(httpContext))
                return false;

            // Kullanýcý giriþ yaptý mý kontrol et
            if (httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
                return false;

            // Session'da birim bilgisi var mý kontrol et
            var userUnit = httpContext.Session["UnitId"];
            return userUnit != null;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                // Yetkisiz eriþim - Login sayfasýna yönlendir
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }
    }

    /// <summary>
    /// Admin yetkilendirme kontrolü
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            var userRole = httpContext.Session["UserRole"];
            return userRole != null && userRole.ToString() == "Admin";
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!AuthorizeCore(filterContext.HttpContext))
            {
                filterContext.Result = new HttpStatusCodeResult(403);
            }
        }
    }
}
