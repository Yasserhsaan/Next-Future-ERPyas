using System;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.Items.Models
{
    public class ItemCost
    {
        [Key]
        public int CostID { get; set; }
        public int ItemID { get; set; }

        // 'L' = Last, 'A' = Average, 'F' = FIFO, 'M' = Moving Average
        public string CostMethod { get; set; } = "A"; // char(1)

        public decimal? LastCost { get; set; }
        public decimal? AvgCost { get; set; }
        public decimal? MinCost { get; set; }
        public decimal? MaxCost { get; set; }

        public decimal StandardCost { get; set; }
        public decimal LastPurchaseCost { get; set; }
        public decimal MovingAverageCost { get; set; }
        public decimal FIFOCost { get; set; }

        public DateTime? LastPurchaseDate { get; set; }
        public DateTime? LastUpdate { get; set; }

        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
    }
}


