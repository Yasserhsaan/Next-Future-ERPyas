using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.InitialSystem.Models;

namespace Next_Future_ERP.Features.PosStations.Models
{
    /// <summary>
    /// نموذج نقاط البيع
    /// </summary>
    [Table("POS_Stations")]
    public class PosStation
    {
        [Key]
        [Column("POS_ID")]
        public int PosId { get; set; }

        [Required]
        [Column("Branch_ID")]
        public int BranchId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("POS_Name")]
        public string PosName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("POS_Code")]
        public string PosCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("GL_Cash_Account")]
        public string GlCashAccount { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("GL_Sales_Account")]
        public string GlSalesAccount { get; set; } = string.Empty;

        [Required]
        [Column("Assigned_User")]
        public int AssignedUser { get; set; }

        [Column("Allowed_Payment_Methods")]
        public string? AllowedPaymentMethods { get; set; }

        [Column("User_Permissions")]
        public string? UserPermissions { get; set; }

        [Column("Is_Active")]
        public bool IsActive { get; set; } = true;

        [Column("Created_Date")]
        public DateTime? CreatedDate { get; set; }

        [Column("Updated_Date")]
        public DateTime? UpdatedDate { get; set; }

        [Column("CompanyId")]
        public int? CompanyId { get; set; }

        // Navigation Properties
        [ForeignKey("BranchId")]
        public virtual BranchModel? Branch { get; set; }

        [ForeignKey("AssignedUser")]
        public virtual Nextuser? AssignedUserNavigation { get; set; }

        // Computed Properties for UI
        [NotMapped]
        public string? BranchName { get; set; }

        [NotMapped]
        public string? AssignedUserName { get; set; }

        [NotMapped]
        public string StatusText => IsActive ? "نشط" : "غير نشط";

        [NotMapped]
        public string StatusColor => IsActive ? "#4caf50" : "#f44336";
    }
}
