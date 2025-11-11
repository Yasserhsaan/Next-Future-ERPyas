using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Models
{
    public class ValuationGroupAccount
    {
        public int ValuationGroupAccountsId { get; set; } // maps to [ValuationGroupAccounts]
        public int ValuationGroup { get; set; }           // مجرد int (لا علاقة EF)
        public int CompanyId { get; set; }

        public string? InventoryAcc { get; set; }
        public string? COGSAcc { get; set; }
        public string? SalesAcc { get; set; }
        public string? SalesDiscountAcc { get; set; }
        public string? LossAcc { get; set; }
        public string? AdjustmentAcc { get; set; }
        public string? EarnedDiscountAccount { get; set; }
        public string? ExpenseAcc { get; set; }
        public string? TaxAccPurchase { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
