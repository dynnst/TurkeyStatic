using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    /// <summary>
    /// GBT (Genel Biliþim Taramasý) ve YTS (Yabancý Taramasý) Sorgu Ýstatistikleri
    /// </summary>
    public class QueryStatistic
    {
        [Key]
        public int QueryStatisticId { get; set; }

        [Required]
        public int UnitId { get; set; }
public virtual Unit Unit { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Shift { get; set; } // Vardia/Büro bilgisi

        [Range(0, int.MaxValue)]
        public int PersonQueriedCount { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int PersonArrestedSearchedCount { get; set; } = 0;

        [StringLength(50)]
        public string OperationType { get; set; } // Aylýk/Günlük takibat

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


