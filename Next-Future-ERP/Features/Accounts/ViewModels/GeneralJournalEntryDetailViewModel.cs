using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public class GeneralJournalEntryDetailViewModel : ObservableObject
    {
        private GeneralJournalEntryDetail _detail;

        public GeneralJournalEntryDetailViewModel()
        {
            _detail = new GeneralJournalEntryDetail
            {
                ExchangeRate = 1.000000m
            };
        }

        public GeneralJournalEntryDetailViewModel(GeneralJournalEntryDetail detail)
        {
            _detail = detail;
        }

        public long DetailId
        {
            get => _detail.DetailId;
            set => SetProperty(_detail.DetailId, value, _detail, (d, v) => d.DetailId = v);
        }

        public string AccountNumber
        {
            get => _detail.AccountNumber;
            set => SetProperty(_detail.AccountNumber, value, _detail, (d, v) => d.AccountNumber = v);
        }

        public int? CostCenterId
        {
            get => _detail.CostCenterId;
            set => SetProperty(_detail.CostCenterId, value, _detail, (d, v) => d.CostCenterId = v);
        }

        public string Statement
        {
            get => _detail.Statement;
            set => SetProperty(_detail.Statement, value, _detail, (d, v) => d.Statement = v);
        }

        public decimal? DebitAmount
        {
            get => _detail.DebitAmount;
            set => SetProperty(_detail.DebitAmount, value, _detail, (d, v) => d.DebitAmount = v);
        }

        public decimal? CreditAmount
        {
            get => _detail.CreditAmount;
            set => SetProperty(_detail.CreditAmount, value, _detail, (d, v) => d.CreditAmount = v);
        }

        public int CurrencyId
        {
            get => _detail.CurrencyId;
            set => SetProperty(_detail.CurrencyId, value, _detail, (d, v) => d.CurrencyId = v);
        }

        public decimal ExchangeRate
        {
            get => _detail.ExchangeRate;
            set => SetProperty(_detail.ExchangeRate, value, _detail, (d, v) => d.ExchangeRate = v);
        }

        public GeneralJournalEntryDetail GetModel() => _detail;
    }
}