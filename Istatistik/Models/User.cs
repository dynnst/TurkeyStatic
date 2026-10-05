using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Istatistik.Models
{
    /// <summary>
    /// Sistem Kullanıcıları
    /// </summary>
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        public string FullName { get; set; }

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }

        public int UnitId { get; set; }
public virtual Unit Unit { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } // Admin, DataEntryPersonnel, Observer

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }
    }

    /// <summary>
    /// Kullanıcı Rolleri
    /// </summary>
    public class UserRole
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string DataEntry = "DataEntryPersonnel";
            public const string Observer = "Observer";
        }
    }
}


