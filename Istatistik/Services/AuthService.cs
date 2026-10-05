using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Istatistik.Models;

namespace Istatistik.Services
{
    /// <summary>
    /// Session'daki oturum bilgisi. Havalimanı (Border) her zaman buradan okunur, istemciden alınmaz.
    /// </summary>
    public class CurrentUser
    {
        public string Sicil { get; set; }
        public string Role { get; set; }
        public string Border { get; set; }
        public List<string> BureauCodes { get; set; } = new List<string>();

        public bool IsSuperAdmin => Role == AppRoles.SuperAdmin;
        public bool IsUnitAdmin => Role == AppRoles.UnitAdmin;

        public bool CanAccessBureau(string code)
        {
            if (IsSuperAdmin || IsUnitAdmin) return true;
            return BureauCodes.Contains(code);
        }

        public static CurrentUser FromSession(HttpSessionStateBase session)
        {
            if (session == null) return null;
            var role = session["UserRole"] as string;
            if (string.IsNullOrEmpty(role)) return null;

            return new CurrentUser
            {
                Sicil = session["Username"] as string,
                Role = role,
                Border = session["Border"] as string,
                BureauCodes = (session["BureauCodes"] as List<string>) ?? new List<string>()
            };
        }
    }

    public class AuthResult
    {
        public int UserId { get; set; }
        public string Sicil { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Border { get; set; }
        public List<string> BureauCodes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Kimlik E_User tablosundan, yetki UserAssignment tablosundan okunur.
    /// </summary>
    public static class AuthService
    {
        public static AuthResult Authenticate(string sicil, string password)
        {
            try
            {
                sicil = (sicil ?? "").Trim();

                using (var ceza = new CezaContext())
                {
                    var e = ceza.EUsers.FirstOrDefault(u => u.fldSicil == sicil);
                    if (e == null || e.IsActive != true || e.fldActive != "1")
                        return null;

                    if (!IdentityPasswordVerifier.Verify(e.fldPassword, password))
                        return null;

                    var border = (e.fldBorder ?? "").Trim();
                    var result = new AuthResult
                    {
                        UserId = e.fldId,
                        Sicil = e.fldSicil,
                        Email = e.fldEmail,
                        FullName = ((e.fldName ?? "") + " " + (e.fldLastName ?? "")).Trim(),
                        Border = border
                    };

                    if (IsSuperAdminSicil(sicil))
                    {
                        result.Role = AppRoles.SuperAdmin;
                        return result;
                    }

                    using (var db = new IstatistikContext())
                    {
                        var a = db.UserAssignments.FirstOrDefault(x => x.Sicil == sicil && x.IsActive);

                        // Yönetim ekranından atanan SuperAdmin tüm havalimanlarına erişir
                        if (a != null && a.Role == AppRoles.SuperAdmin)
                        {
                            result.Role = AppRoles.SuperAdmin;
                            return result;
                        }

                        if (a == null || string.IsNullOrEmpty(border) ||
                            !string.Equals(a.Border, border, StringComparison.OrdinalIgnoreCase))
                            return null;

                        result.Role = a.Role;
                        result.BureauCodes = db.UserBureaus
                            .Where(ub => ub.UserAssignmentId == a.Id && ub.Bureau.IsActive)
                            .Select(ub => ub.Bureau.Code)
                            .ToList();
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsSuperAdminSicil(string sicil)
        {
            var list = System.Configuration.ConfigurationManager.AppSettings["SuperAdminSicils"] ?? "";
            return list.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Contains(sicil);
        }
    }
}
