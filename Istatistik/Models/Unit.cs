using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Istatistik.Models
{
    /// <summary>
    /// Birim (Departman) Modeli
    /// Suç Önleme, Ýdari Büro, Pasaport Büro, vb.
    /// </summary>
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }

        [Required]
        [StringLength(100)]
        public string UnitName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

