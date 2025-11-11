using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Permissions.Models
{
    [Table("MenwRolls")]
    public class MenuRole
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Fromdid")]
        public int FormId { get; set; }

        [Column("Rollid")]
        public int RoleId { get; set; }

        [Column("Dbtimestamp")]
        [Timestamp]
        public byte[] DbTimestamp { get; set; } 

        // Navigation properties
        public virtual MenuForm MenuForm { get; set; } = null!;
        public virtual SysRole SysRole { get; set; } = null!;
    }
}
