// Models/Bank.cs
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Models
{
    public class Bank
    {
        public int BankId { get; set; }

        // علاقات مع الشركة والفرع
        public int CompanyId { get; set; }
        public int BranchId { get; set; }

        [Required, MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        // هذا رقم الحساب في الدليل المحاسبي (كود حساب من شجرة الحسابات - Accounts)
        [Required, MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ContactInfo { get; set; }

        public bool? IsActive { get; set; } = true;

        // إيقاف البنك (تاريخ + سبب)
        public DateTime? StopDate { get; set; }   // SQL: date (نحددها بالـ Fluent)
        [MaxLength(255)]
        public string? StopReason { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigations
        public CompanyInfoModel? Company { get; set; }
        public BranchModel? Branch { get; set; }

        public ICollection<BankCurrencyDetail> CurrencyDetails { get; set; } = new List<BankCurrencyDetail>();
    }
}
