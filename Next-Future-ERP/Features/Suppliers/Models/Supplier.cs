namespace Next_Future_ERP.Features.Suppliers.Models
{
    public class Supplier
    {
        public int SupplierID { get; set; }    // Identity
        public string SupplierCode { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
        public string TaxNumber { get; set; } = null!;

        public int AccountID { get; set; }
        public int? CostCenterID { get; set; }
        public int? PaymentTerms { get; set; }    // FK -> Payment_Terms.Term_ID (اختياري)
        public decimal? CreditLimit { get; set; }

        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsBlacklisted { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string? Nationality { get; set; }
        public string? IDNumber { get; set; }
        public string? CRNumber { get; set; }
        public string? VATNumber { get; set; }

        public int? DefaultPaymentMethodID { get; set; }  // يعكس الافتراضي من جدول الربط

        // Navigation
        public ICollection<SupplierPaymentMethod> PaymentMethods { get; set; } = new List<SupplierPaymentMethod>();
    }
}
