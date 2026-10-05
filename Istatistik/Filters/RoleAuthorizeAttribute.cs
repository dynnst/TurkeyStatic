using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Istatistik.Filters
{
    /// <summary>
    /// Session'daki UserRole değerine göre rol bazlı yetkilendirme
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[0];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
                return false;

            var role = httpContext.Session?["UserRole"] as string;
            return role != null && _allowedRoles.Contains(role);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
                filterContext.Result = new RedirectResult("~/Account/Login");
            else
                filterContext.Result = new HttpStatusCodeResult(403);
        }
    }
}
