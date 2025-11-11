using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;

namespace Next_Future_ERP.Features.Suppliers.Models
{
    public class SupplierPaymentMethod
    {
        public int SupplierID { get; set; }
        public int Method_ID { get; set; }
        public bool Is_Default { get; set; }

        // Navigation (للعرض)
        public PaymentMethod? Method { get; set; }
    }
}