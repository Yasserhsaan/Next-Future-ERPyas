// Features/Accounts/Models/DebitCreditNotification.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Features.Accounts.Models
{
    // نوع الإشعار للـ UI
    public enum DebitCreditType { Debit, Credit } // D / C

    public class DebitCreditNotification
    {
        [Key] public long NotificationId { get; set; }

        public int CompanyId { get; set; } = 1; // ثابت الآن (يمكن تمريره من Session)
        public int BranchId { get; set; }

        // في الجدول Char(1): 'D' أو 'C'
        public string NotificationType { get; set; } = "D";

        public DateTime NotificationDate { get; set; } = DateTime.Today;
        public string AccountNumber { get; set; } = string.Empty;

        public int CurrencyId { get; set; }
        public byte Status { get; set; } = 0;

        public decimal TotalAmount { get; set; } // (18,4)

        public DateTime? PostingDate { get; set; }
        public DateTime? AmendmentDate { get; set; }

        public int CreatedBy { get; set; } = 1;
        public DateTime? CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public List<DebitCreditNoteDetail> Details { get; set; } = new();
    }

    public class DebitCreditNoteDetail
    {
        [Key] public long DetailId { get; set; }

        public long NotificationId { get; set; }
        public int BranchId { get; set; }

        public DateTime PostingDate { get; set; } = DateTime.Today;
        public string Statement { get; set; } = string.Empty;

        public decimal AmountTransaction { get; set; }  // (18,4)
        public decimal AmountCompany { get; set; }      // (18,4)
        public decimal ExchangeRate { get; set; } = 1m; // (18,6)

        public DateTime? CreatedAt { get; set; }
    }

    // عنصر نتيجة البحث (Lookup)
    public class DebitCreditNotificationLookupItem
    {
        public long NotificationId { get; set; }
        public DateTime NotificationDate { get; set; }
        public string DCType { get; set; } = "D"; // D / C
        public string BranchName { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public string CurrencyName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public byte Status { get; set; }
    }
}
