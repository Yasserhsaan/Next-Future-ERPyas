using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    public class ItemComponent
    {
        [Key]
        public int ItemComponentID { get; set; }
        public int ParentItemID { get; set; }
        public int ComponentItemID { get; set; }
        public int UnitID { get; set; }
        public decimal Quantity { get; set; } = 0m;
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        [NotMapped]
        public string? ComponentItemName { get; set; } // For display purposes
        [NotMapped]
        public string? UnitName { get; set; } // For display purposes
    }
}
