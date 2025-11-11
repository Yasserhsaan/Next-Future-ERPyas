using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Permissions.Models
{
    [Table("MenwFomrs")]
    public class MenuForm
    {
        [Key]
        public int MenuFormCode { get; set; }
       
        public int? MenuFormParent { get; set; }
        
      
        [StringLength(255)]
        public string? ProgramExecutable { get; set; }
        
      
        public string MenuName { get; set; } = string.Empty;
        
        [Column("Visible")]
        public int Visible { get; set; } = 1;
        
        [StringLength(255)]
        public string MenuArabicName { get; set; } = string.Empty;
        
       
        public int NSync { get; set; } = 0;

        [Timestamp]
        public byte[] DbTimestamp { get; set; }

        // Navigation properties
        public virtual MenuForm? Parent { get; set; }
        public virtual ICollection<MenuForm> Children { get; set; } = new List<MenuForm>();
        public virtual ICollection<MenuRole> MenuRoles { get; set; } = new List<MenuRole>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        // Computed properties
        [NotMapped]
        public bool IsVisible => Visible == 1;
        
        [NotMapped]
        public bool IsParent => Children.Any();
        
        [NotMapped]
        public string DisplayName => string.IsNullOrEmpty(MenuArabicName) ? MenuName : MenuArabicName;

        [NotMapped]
        public string ParentMenuName => Parent?.DisplayName ?? "القائمة الرئيسية";

        [NotMapped]
        public string GroupName => MenuFormParent.HasValue ? $"القائمة: {ParentMenuName}" : "القوائم الرئيسية";

        // Form properties for UI
        [NotMapped]
        public bool IsActive { get; set; } = true;
        
        [NotMapped]
        public bool IsProgram { get; set; } = false;
        
        [NotMapped]
        public bool CanView { get; set; } = true;
        
        [NotMapped]
        public bool CanPost { get; set; } = false;
        
        [NotMapped]
        public bool CanPrint { get; set; } = false;
        
        [NotMapped]
        public bool CanRun { get; set; } = true;
    }
}
