using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Next_Future_ERP.Features.Accounts.Models;

namespace Next_Future_ERP.Models
{
    public class Fund
    {
        public int FundId { get; set; }

        // علاقات مع الشركة والفرع (SystemSettings/Branches)
        public int CompanyId { get; set; }
        public int BranchId { get; set; }

        [Required, MaxLength(100)]
        public string FundName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        // وظيفة الصندوق (tinyint)
        public FundType FundType { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsUsed { get; set; }

        public DateTime? StopDate { get; set; }   // SQL: date (نحددها بالـ Fluent)
        [MaxLength(255)]
        public string? StopReason { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigations (اختيارية إن ما تحتاج مجموعات عكسية)
        public CompanyInfoModel? Company { get; set; }
        public BranchModel? Branch { get; set; }

        public ICollection<FundCurrencyLimit> CurrencyLimits { get; set; } = new List<FundCurrencyLimit>();
    }
}
