using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Next_Future_ERP.Models;

namespace Next_Future_ERP.Features.Accounts.Models
{
    [Table("OpeningBalanceLine")]
    public class OpeningBalanceLine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int LineId { get; set; }

        [Required]
        public int BatchId { get; set; }

        private int _accountId;
        [Required]
        public int AccountId
        {
            get => _accountId;
            set
            {
                _accountId = value;
                OnPropertyChanged(nameof(AccountId));
                OnPropertyChanged(nameof(AccountDisplay));
            }
        }

        // خاصية مساعدة للربط مع AccountSearchBox
        [NotMapped]
        public Account? SelectedAccount { get; set; }

        private int _transactionCurrencyId;
        [Required]
        public int TransactionCurrencyId 
        { 
            get => _transactionCurrencyId; 
            set 
            { 
                _transactionCurrencyId = value; 
                OnPropertyChanged(nameof(TransactionCurrencyId));
                OnPropertyChanged(nameof(TransactionCurrencyName));
            } 
        }

        [NotMapped]
        public string TransactionCurrencyName { get; set; } = "غير محدد";

        private decimal _transactionDebit;
        [Column(TypeName = "decimal(18,4)")]
        public decimal TransactionDebit
        {
            get => _transactionDebit;
            set
            {
                _transactionDebit = value;
                if (value > 0) TransactionCredit = 0; // جانب واحد فقط
                OnPropertyChanged(nameof(TransactionDebit));
                RecalculateCompanyAmounts();
            }
        }

        private decimal _transactionCredit;
        [Column(TypeName = "decimal(18,4)")]
        public decimal TransactionCredit
        {
            get => _transactionCredit;
            set
            {
                _transactionCredit = value;
                if (value > 0) TransactionDebit = 0; // جانب واحد فقط
                OnPropertyChanged(nameof(TransactionCredit));
                RecalculateCompanyAmounts();
            }
        }

        [Required]
        public int CompanyCurrencyId { get; set; }

        private decimal _companyDebit;
        [Column(TypeName = "decimal(18,4)")]
        public decimal CompanyDebit
        {
            get => _companyDebit;
            set
            {
                _companyDebit = value;
                OnPropertyChanged(nameof(CompanyDebit));
            }
        }

        private decimal _companyCredit;
        [Column(TypeName = "decimal(18,4)")]
        public decimal CompanyCredit
        {
            get => _companyCredit;
            set
            {
                _companyCredit = value;
                OnPropertyChanged(nameof(CompanyCredit));
            }
        }

        private decimal _exchangeRate = 1;
        [Column(TypeName = "decimal(18,6)")]
        [Required]
        public decimal ExchangeRate
        {
            get => _exchangeRate;
            set
            {
                _exchangeRate = value;
                OnPropertyChanged(nameof(ExchangeRate));
                RecalculateCompanyAmounts();
            }
        }

        [StringLength(200)]
        public string? Note { get; set; }

        public int? CostCenterId { get; set; }

        // Navigation properties (not mapped)
        private string? _accountCode;
        [NotMapped]
        public string? AccountCode 
        { 
            get => _accountCode; 
            set 
            { 
                _accountCode = value; 
                OnPropertyChanged(nameof(AccountCode));
                OnPropertyChanged(nameof(AccountDisplay));
            } 
        }

        private string? _accountNameAr;
        [NotMapped]
        public string? AccountNameAr 
        { 
            get => _accountNameAr; 
            set 
            { 
                _accountNameAr = value; 
                OnPropertyChanged(nameof(AccountNameAr));
                OnPropertyChanged(nameof(AccountDisplay));
            } 
        }

        [NotMapped]
        public string AccountDisplay => !string.IsNullOrEmpty(AccountCode) && !string.IsNullOrEmpty(AccountNameAr) 
            ? $"{AccountCode} — {AccountNameAr}" 
            : "غير محدد";

        [NotMapped]
        public string? CostCenterName { get; set; }


        [NotMapped]
        public string? CompanyCurrencyName { get; set; }

        [NotMapped]
        public bool UsesCostCenter { get; set; }

        [MaxLength(500)]
        public string? Statement { get; set; } = "رصيد افتتاحي";

        [NotMapped]
        public bool IsCostCenterRequired => UsesCostCenter && CostCenterId == null;

        [NotMapped]
        public decimal TransactionNet => TransactionDebit - TransactionCredit;

        [NotMapped]
        public decimal CompanyNet => CompanyDebit - CompanyCredit;

        [NotMapped]
        public bool IsValid => AccountId > 0 && 
                              TransactionCurrencyId > 0 && 
                              CompanyCurrencyId > 0 &&
                              ExchangeRate > 0 &&
                              (TransactionDebit > 0 || TransactionCredit > 0) &&
                              !(TransactionDebit > 0 && TransactionCredit > 0) &&
                              (!UsesCostCenter || CostCenterId.HasValue);

        private void RecalculateCompanyAmounts()
        {
            if (ExchangeRate > 0)
            {
                CompanyDebit = Math.Round(TransactionDebit * ExchangeRate, 4);
                CompanyCredit = Math.Round(TransactionCredit * ExchangeRate, 4);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
