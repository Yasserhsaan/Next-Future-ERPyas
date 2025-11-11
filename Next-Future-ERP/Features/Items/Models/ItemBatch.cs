using System;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.Items.Models
{
    public class ItemBatch
    {
        [Key]
        public int BatchID { get; set; }
        public int ItemID { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? LocationInWarehouse { get; set; }
        public decimal? CurrentQuantity { get; set; }
        public string? BatchStatus { get; set; } // 'A','C','R'
        public decimal? ReservedQuantity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
    }
}


