using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Next_Future_ERP.Data.Models; // For Nextuser
using Next_Future_ERP.Features.PosStations.Models; // For PosStation

namespace Next_Future_ERP.Features.PosOperators.Models
{
    /// <summary>
    /// نموذج مشغلي نقاط البيع
    /// </summary>
    [Table("POS_Operators")]
    public class PosOperator
    {
        [Key]
        [Column("OperatorId")]
        public int OperatorId { get; set; }

        [Required]
        [Column("POS_ID")]
        public int PosId { get; set; }

        [Required]
        [Column("User_ID")]
        public int UserId { get; set; }

        [Column("IsPrimary")]
        public bool IsPrimary { get; set; } = false;

        [Column("StartDate")]
        public DateTime? StartDate { get; set; } = DateTime.Now;

        [Column("EndDate")]
        public DateTime? EndDate { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CompanyId")]
        public int? CompanyId { get; set; }

        [Column("BranchId")]
        public int? BranchId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(PosId))]
        public virtual PosStation? PosStation { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Nextuser? User { get; set; }

        // Display properties (not mapped to DB)
        [NotMapped]
        public string? PosStationName { get; set; }

        [NotMapped]
        public string? UserName { get; set; }

        [NotMapped]
        public string StatusText => IsActive ? "نشط" : "غير نشط";

        [NotMapped]
        public string PrimaryText => IsPrimary ? "رئيسي" : "عادي";

        [NotMapped]
        public System.Windows.Media.Brush StatusColor => IsActive ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
    }
}
