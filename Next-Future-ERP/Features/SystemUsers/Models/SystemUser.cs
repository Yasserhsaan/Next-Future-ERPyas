using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Next_Future_ERP.Data.Models; // For Nextuser
using Next_Future_ERP.Features.InitialSystem.Models; // For BranchModel
using Next_Future_ERP.Features.Permissions.Models; // For SysRole

namespace Next_Future_ERP.Features.SystemUsers.Models
{
    /// <summary>
    /// نموذج مستخدمي النظام
    /// </summary>
    [Table("Nextuser")]
    public class SystemUser
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("Code")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column("Name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("Fname")]
        [StringLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Column("Mobile")]
        [StringLength(255)]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [Column("Phone")]
        [StringLength(255)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Column("Address")]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Column("Email")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("Password")]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("Nsync")]
        public int Nsync { get; set; }

        [Column("Dbtimestamp")]
        public byte[]? DbTimestamp { get; set; }

        [Column("UserRollid")]
        public int? UserRoleId { get; set; }

        [Required]
        [Column("PasswordHash")]
        [StringLength(128)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("PasswordSalt")]
        [StringLength(128)]
        public string PasswordSalt { get; set; } = string.Empty;

        [Column("LastLogin")]
        public DateTime? LastLogin { get; set; }

        [Column("FailedLoginAttempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [Column("IsLocked")]
        public bool IsLocked { get; set; } = false;

        [Column("LockoutEnd")]
        public DateTime? LockoutEnd { get; set; }

        [Column("userJob")]
        [StringLength(255)]
        public string? UserJob { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(BranchId))]
        public virtual BranchModel? Branch { get; set; }

        [ForeignKey(nameof(UserRoleId))]
        public virtual SysRole? UserRole { get; set; }

        // Display properties (not mapped to DB)
        [NotMapped]
        public string? BranchName { get; set; }

        [NotMapped]
        public string? RoleName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {Name}";

        [NotMapped]
        public string StatusText => IsLocked ? "مقفل" : "نشط";

        [NotMapped]
        public string LockStatusText => IsLocked ? "مقفل" : "مفتوح";

        [NotMapped]
        public System.Windows.Media.Brush StatusColor => IsLocked ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;

        [NotMapped]
        public string LastLoginText => LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "لم يسجل دخول";
    }
}
