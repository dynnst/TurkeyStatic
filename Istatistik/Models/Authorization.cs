using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string UnitAdmin = "UnitAdmin";
        public const string BureauUser = "BureauUser";
    }

    public static class BureauCodes
    {
        public const string Pasaport = "PASAPORT";
        public const string Idari = "IDARI";
        public const string Guvenlik = "GUVENLIK";
        public const string BilgiTeknolojileri = "BILGI_TEK";
        public const string Trafik = "TRAFIK";
        public const string GbtUyap = "GBT_UYAP";
        public const string SucOnleme = "SUC_ONLEME";
        public const string YtsSorgu = "YTS_SORGU";
        public const string SeyahatBelgeRisk = "SEYAHAT_BELGE_RISK";
    }

    /// <summary>
    /// Havalimanı (E_User.fldBorder) bazlı ayrılan kayıtlar
    /// </summary>
    public interface IBorderEntity
    {
        string Border { get; set; }
    }

    /// <summary>
    /// Havalimanına ait büro (Pasaport, Suç Önleme, Trafik vb.)
    /// </summary>
    public class Bureau
    {
        [Key]
        public int BureauId { get; set; }

        [Required]
        [StringLength(150)]
        [Index("IX_Bureau_Border_Code", 1, IsUnique = true)]
        public string Border { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_Bureau_Border_Code", 2, IsUnique = true)]
        public string Code { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// E_User kullanıcısının bu sistemdeki yetkisi
    /// </summary>
    public class UserAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Index(IsUnique = true)]
        public string Sicil { get; set; }

        [Required]
        [StringLength(150)]
        [Index]
        public string Border { get; set; }

        [Required]
        [StringLength(30)]
        public string Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string CreatedBy { get; set; }
    }

    /// <summary>
    /// Kullanıcının yetkili olduğu bürolar
    /// </summary>
    public class UserBureau
    {
        [Key]
        public int Id { get; set; }

        [Index("IX_UserBureau", 1, IsUnique = true)]
        public int UserAssignmentId { get; set; }

        [Index("IX_UserBureau", 2, IsUnique = true)]
        public int BureauId { get; set; }

        [ForeignKey("UserAssignmentId")]
        public virtual UserAssignment UserAssignment { get; set; }

        [ForeignKey("BureauId")]
        public virtual Bureau Bureau { get; set; }
    }
}
