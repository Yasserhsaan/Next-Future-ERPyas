using System;

namespace Next_Future_ERP.Features.Items
{
    // صف معروض في DataGrid
    public class ItemPriceDto
    {
        public int PriceID { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int ItemID { get; set; }
        public string ItemCode { get; set; } = "";
        public string ItemName { get; set; } = "";

        public byte PriceType { get; set; }
        public string PriceTypeName { get; set; } = "";

        public int PriceLevelId { get; set; }
        public string PriceLevelName { get; set; } = "";

        public byte Method { get; set; }
        public string PriceMethodName { get; set; } = "";

        public int UnitID { get; set; }
        public string UnitName { get; set; } = "";

        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }

        public decimal? PriceAmount { get; set; }
        public decimal? PricePercent { get; set; }
        public decimal SellPrice { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedBy { get; set; }
    }
}

