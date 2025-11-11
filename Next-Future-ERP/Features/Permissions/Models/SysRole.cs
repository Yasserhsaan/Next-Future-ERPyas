using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Permissions.Models
{
    [Table("SYSROLLS")]
    public class SysRole
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("RollType")]
        public int RollType { get; set; } = 1; // 1=Admin, 2=Accountant, 3=User

     

        [Column("Dbtimestamp")]
        [Timestamp]
        public byte[] DbTimestamp { get; set; } 

        // Navigation properties
        public virtual ICollection<MenuRole> MenuRoles { get; set; } = new List<MenuRole>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        // Helper properties
        [NotMapped]
        public string RollTypeName => RollType switch
        {
            1 => "إداري",
            2 => "محاسب", 
            3 => "مستخدم",
            _ => "غير محدد"
        };

        [NotMapped]
        public string RollTypeNameEn => RollType switch
        {
            1 => "Administrator",
            2 => "Accountant",
            3 => "User", 
            _ => "Undefined"
        };

        [NotMapped]
        public bool IsActive { get; set; } = true;
    }
}
