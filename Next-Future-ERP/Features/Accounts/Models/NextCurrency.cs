using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Next_Future_ERP.Models
{
    public class NextCurrency : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // خاصية تحكم عرض الأخطاء
        public static bool ValidateNow { get; set; } = false;

        [Key]
        public int CurrencyId { get; set; }

        public int CompanyId { get; set; } = 1; // القيمة الافتراضية

        private string currencyNameAr;
        public string CurrencyNameAr
        {
            get => currencyNameAr;
            set
            {
                currencyNameAr = value;
                OnPropertyChanged(nameof(CurrencyNameAr));
            }
        }

        private string currencyNameEn;
        public string CurrencyNameEn
        {
            get => currencyNameEn;
            set
            {
                currencyNameEn = value;
                OnPropertyChanged(nameof(CurrencyNameEn));
            }
        }

        public string CurrencySymbol { get; set; }
        public string FractionUnit { get; set; }
        public byte? DecimalPlaces { get; set; }
        public bool? IsCompanyCurrency { get; set; }
        public bool? IsForeignCurrency { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? MinExchangeRate { get; set; }
        public decimal? MaxExchangeRate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // للتحقق الشرطي
        public string this[string columnName]
        {
            get
            {
                if (!ValidateNow) return null;

                return columnName switch
                {
                    nameof(CurrencyNameAr) when string.IsNullOrWhiteSpace(CurrencyNameAr) => "الاسم بالعربية مطلوب",
                    nameof(CurrencyNameEn) when string.IsNullOrWhiteSpace(CurrencyNameEn) => "الاسم بالإنجليزية مطلوب",
                    _ => null
                };
            }
        }

        public string Error => null;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
