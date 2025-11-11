using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Next_Future_ERP.Features.Items.Models
{
    public class ItemSupplier
    {
        [Key]
        public int ItemSupplierID { get; set; }
        public int ItemID { get; set; }
        public int SupplierID { get; set; }
        public decimal SupplierPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public bool IsPrimarySupplier { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        [NotMapped]
        public string? SupplierName { get; set; }
        
        [NotMapped]
        public string? CurrencyName { get; set; }
    }
}


