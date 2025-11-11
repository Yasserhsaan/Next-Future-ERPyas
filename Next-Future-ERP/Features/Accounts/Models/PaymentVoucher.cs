using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Next_Future_ERP.Models
{
    public class PaymentVoucher
    {
        [Key]
        public int VoucherID { get; set; }

        public int BranchID { get; set; }
        public string VoucherType { get; set; } = "Cash"; // يخزن نصاً (Cash/Cheque) ليتوافق مع الجدول

        public int? CashBoxID { get; set; }   // صندوق (FundId)
        public int? BankID { get; set; }      // بنك

        public int CurrencyID { get; set; }
        public decimal? ExchangeRate { get; set; } // decimal(18,6)

        public int DocumentTypeID { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime DocumentDate { get; set; } = DateTime.Today;

        public decimal LocalAmount { get; set; }    // decimal(18,3)
        public decimal? ForeignAmount { get; set; } // decimal(18,3)

        public int? CostCenterID { get; set; }
        public string? Statement { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? ReferenceName { get; set; }
        public int? AttachmentsCount { get; set; }
        public string Beneficiary { get; set; } = string.Empty;

        public int? SourceDocTypeID { get; set; }
        public string? SourceDocNumber { get; set; }
        public string? ChequeNumber { get; set; }
        public DateTime? ChequeDueDate { get; set; }

        public string? PostingMethod { get; set; } // طريقة ترحيل الشيكات
        public bool? IsReviewed { get; set; }
        public bool? IsPosted { get; set; }
        public bool? IsSuspended { get; set; }

        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? SyncFlag { get; set; }

        // ملاحة
        public List<PaymentVoucherDetail> Details { get; set; } = new();
    }
}
