using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Models
{
    public class BankCurrencyDetail
    {
        public int DetailId { get; set; }

        // FKs
        public int BankId { get; set; }
        public int CurrencyId { get; set; } // يربط بـ NextCurrencies

        // ملاحظة مهمة: هذا حساب البنك الفعلي لدى البنك (IBAN/رقم حساب بنكي)، وليس حساب الدليل
        public string BankAccountNumber { get; set; } = string.Empty;

        public decimal? MinCash { get; set; }
        public decimal? MaxCash { get; set; }
        public decimal? MinTransaction { get; set; }
        public decimal? MaxTransaction { get; set; }

        public bool? AllowLimitExceed { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigations
        public Bank? Bank { get; set; }
        public NextCurrency? Currency { get; set; }
    }
}