using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    /// <summary>
    /// Audit Log: Kim, Ne Zaman, Hangi Veriyi Deðiþtirdi
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int? UnitId { get; set; }
public virtual Unit Unit { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } // Create, Update, Delete

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } // CrimeStatistic, QueryStatistic, vb.

        [Required]
        public int EntityId { get; set; }

        [StringLength(1000)]
        public string OldValue { get; set; }

        [StringLength(1000)]
        public string NewValue { get; set; }

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string IPAddress { get; set; }
    }
}


