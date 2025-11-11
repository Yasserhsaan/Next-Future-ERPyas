using System;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.Warehouses.Models
{
    public enum WarehouseType : byte
    {
        None = 0, Central = 1, Branch = 2, Sales = 3
    }

    public class Warehouse
    {
        [Key] public int WarehouseID { get; set; }

        [Required, MaxLength(20)]
        public string WarehouseCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string WarehouseName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        public int? ManagerID { get; set; }
        public int? CompanyId { get; set; }
        public int? BranshId { get; set; }

        public bool? AllowNegativeStock { get; set; }
        public bool? UseBins { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public WarehouseType? WarehouseType { get; set; }

        public bool? UseReciveTransctions { get; set; }
        public bool? UseIsuuseTransctions { get; set; }
        public bool? UseTransferTransctions { get; set; }
        public bool? UseReturnTransctions { get; set; }
        public bool? UseCountTransctions { get; set; }
        public int? DefultCostCenter { get; set; }
        public bool? UseSalesTansctions { get; set; }
    }
}
