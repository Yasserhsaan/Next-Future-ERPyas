using System;

namespace Next_Future_ERP.Models
{
    public class FundCurrencyLimit
    {
        public int LimitId { get; set; }

        // مفاتيح أجنبية
        public int FundId { get; set; }
        public int CurrencyId { get; set; } // سنربطه بـ NextCurrencies

        // حدود نقدية وتسوية
        public decimal? MinCash { get; set; }
        public decimal? MaxCash { get; set; }
        public decimal? MinSettlement { get; set; }
        public decimal? MaxSettlement { get; set; }

        public bool? AllowLimitExceed { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigations
        public Fund? Fund { get; set; }
        public NextCurrency? Currency { get; set; } // إن أردت الربط بـ Currencies غيّر النوع والـ mapping
    }
}
