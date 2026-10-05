using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    /// <summary>
    /// Suç Önleme Faaliyetleri ve Arama/Yakalamalar
    /// </summary>
    public class CrimePreventionActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        public int UnitId { get; set; }
public virtual Unit Unit { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [StringLength(50)]
        public string WarrantSource { get; set; } // UAP/GBT kaynaklı

        [Range(0, int.MaxValue)]
        public int WarrantCount { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int ApprehendedCount { get; set; } = 0;

        [StringLength(200)]
        public string ApprehensionLocation { get; set; } // Yakalama Yeri/Noktası

        [Range(0, int.MaxValue)]
        public int ArrestedCount { get; set; } = 0;

        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        [StringLength(100)]
        public string ModifiedBy { get; set; }
    }
}


