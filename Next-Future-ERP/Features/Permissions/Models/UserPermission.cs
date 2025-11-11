using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Next_Future_ERP.Features.Permissions.Models
{
    [Table("UsersPermissions")]
    public partial class UserPermission : ObservableObject
    {
        [Key]
        [Column("PerID")]
        public int PerId { get; set; }

        [Column("BranchId")]
        public int BranchId { get; set; }

        [Column("ComiId")]
        public int CompanyId { get; set; }

        [Column("UserID")]
        public int UserId { get; set; }

        [Column("Rollid")]
        public int RoleId { get; set; }

        [ObservableProperty]
        [Column("AllowAdd")]
        private bool allowAdd = false;

        [ObservableProperty]
        [Column("AllowEdit")]
        private bool allowEdit = false;

        [ObservableProperty]
        [Column("AllowDel")]
        private bool allowDelete = false;

        [Column("FormID")]
        public int FormId { get; set; }

        [ObservableProperty]
        [Column("AllowView")]
        private bool allowView = false;

        [ObservableProperty]
        [Column("AllowPost")]
        private bool allowPost = false;

        [ObservableProperty]
        [Column("AllowPrint")]
        private bool allowPrint = false;

        [ObservableProperty]
        [Column("AllowRun")]
        private bool allowRun = false;

        // Navigation properties
        public virtual MenuForm MenuForm { get; set; } = null!;
        public virtual SysRole SysRole { get; set; } = null!;
        public virtual Next_Future_ERP.Data.Models.Nextuser User { get; set; } = null!;

        // Helper properties
        [NotMapped]
        public bool HasAnyPermission => AllowAdd || AllowEdit || AllowDelete || AllowView || AllowPost || AllowPrint || AllowRun;

        [NotMapped]
        public string PermissionSummary => string.Join(", ", 
            new[] 
            {
                AllowAdd ? "إضافة" : null,
                AllowEdit ? "تعديل" : null,
                AllowDelete ? "حذف" : null,
                AllowView ? "عرض" : null,
                AllowPost ? "ترحيل" : null,
                AllowPrint ? "طباعة" : null,
                AllowRun ? "تشغيل" : null
            }.Where(x => x != null));
    }
}
