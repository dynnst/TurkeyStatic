using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    /// <summary>
    /// CEZA veritabanındaki mevcut E_User tablosu (salt okunur kullanım)
    /// </summary>
    [Table("E_User")]
    public class EUser
    {
        [Key]
        public int fldId { get; set; }

        public string fldSicil { get; set; }
        public string fldName { get; set; }
        public string fldLastName { get; set; }
        public string fldEmail { get; set; }
        public string fldPassword { get; set; }
        public string fldBorder { get; set; }
        public string fldActive { get; set; }
        public string Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
