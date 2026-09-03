using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    /// <summary>
    /// Suç Önleme Biriminin Ceraim (Olay/Suç) Ýstatistikleri
    /// </summary>
    public class CrimeStatistic
    {
        [Key]
        public int CrimeStatisticId { get; set; }

        [Required]
        public int UnitId { get; set; }
public virtual Unit Unit { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(100)]
        public string CrimeType { get; set; }

        [StringLength(20)]
        public string CrimeCode { get; set; }

        [Range(0, int.MaxValue)]
        public int CasesRequiringFollow_up { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int SuspectCount { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int ArrestedCount { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int JudicialControlCount { get; set; } = 0;

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


