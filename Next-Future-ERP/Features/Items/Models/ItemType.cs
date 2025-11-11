using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    public class ItemType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemTypeID { get; set; }

        [Required]
        [StringLength(1)]
        public string ItemTypeCode { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string ItemTypeNameAr { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string ItemTypeNameEn { get; set; } = "";

        [Required]
        public int RangeStart { get; set; }

        [Required]
        public int RangeEnd { get; set; }

        [StringLength(50)]
        public string? CategoryType { get; set; }

        // Navigation property for display
        [NotMapped]
        public string DisplayName => $"{ItemTypeNameAr} ({ItemTypeCode})";
    }
}
