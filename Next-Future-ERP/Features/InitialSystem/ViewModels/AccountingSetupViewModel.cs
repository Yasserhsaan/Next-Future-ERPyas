using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class AccountingSetupViewModel : ObservableValidator, IWizardStepValidation
    {
        private readonly WizardState _wizardState;

        [ObservableProperty]
        [Required(ErrorMessage = "الفرع مطلوب")]
        private string branchName;

        [ObservableProperty]
        [Required(ErrorMessage = "السنة المالية مطلوبة")]
        [RegularExpression("^[0-9]{4}$", ErrorMessage = "السنة يجب أن تكون 4 أرقام")]
        private string fiscalYear;

        // Replace the problematic line with the following:
        public ObservableCollection<int> Months { get; } = new(Enumerable.Range(1, 12)
        .Select(i =>
        {

            return i;
        }).ToList());

        [ObservableProperty]
        [Required(ErrorMessage = "الشهر مطلوب")]
        [Range(1, 12, ErrorMessage = "الشهر يجب أن يكون من 1 إلى 12")]
        private int currentMonth;

        [ObservableProperty]
        private string defaultCashAccount;

        [ObservableProperty]
        private string defaultBankAccount;

        [ObservableProperty]
        private string defaultInventoryAccount;

        [ObservableProperty]
        private string profitAndLossAccount;

        [ObservableProperty]
        private bool autoPosting;

        [ObservableProperty]
        private string selectedChartType;


        public AccountingSetupViewModel(WizardState wizardState)
        {
            _wizardState = wizardState;

            if (wizardState.AccountingSetup != null) {
                BranchName = wizardState.Branches.BranchName ; // Default to 1 if not set
                FiscalYear = wizardState.CompanyInfo?.FiscalYearStart ?? DateTime.Now.Year.ToString(); // Default to current year
                CurrentMonth = wizardState.AccountingSetup?.CurrentMonth ?? 1; // Default to January
                DefaultCashAccount = wizardState.AccountingSetup?.DefaultCashAccount ?? "0"; // Default account number
                DefaultBankAccount = wizardState.AccountingSetup?.DefaultBankAccount ?? "0"; // Default account number
                DefaultInventoryAccount = wizardState.AccountingSetup?.DefaultInventoryAccount ?? "0"; // Default account number
                ProfitAndLossAccount = wizardState.AccountingSetup?.ProfitLossAccount ?? "0"; // Default account number
                AutoPosting = wizardState.AccountingSetup?.AutoPosting ?? false; // Default to false
               // SelectedChartType = wizardState.AccountingSetup?.SelectedChartType ?? "Standard"; // Default chart type
            }
            else
            {
                _wizardState.AccountingSetup = new AccountingSetupModel();
            }

        }


        public bool ValidateStep(out string? errorMessage)
        {
            ValidateAllProperties();

            var errors = new[]
            {
                GetErrors(nameof(BranchName)).Cast<ValidationResult>().FirstOrDefault(),



            }
            .Where(e => e != null)
            .Select(e => e!.ErrorMessage)
            .ToList();

            if (errors.Any())
            {
                errorMessage = errors.First();
                return false;
            }



            _wizardState.AccountingSetup.BranchId  = _wizardState.Branches?.BranchId ?? 1; // Default to 1 if not set
            _wizardState.AccountingSetup.FiscalYear = Convert.ToUInt16(FiscalYear);
            _wizardState.AccountingSetup.CurrentMonth = CurrentMonth;
            _wizardState.AccountingSetup.DefaultCashAccount = DefaultCashAccount;
            _wizardState.AccountingSetup.DefaultBankAccount = DefaultBankAccount;
            _wizardState.AccountingSetup.DefaultInventoryAccount = DefaultInventoryAccount;
            _wizardState.AccountingSetup.ProfitLossAccount = ProfitAndLossAccount;
            _wizardState.AccountingSetup.AutoPosting = AutoPosting;
            _wizardState.AccountingSetup.ComiId = 1; // Default to 0 if not set








            errorMessage = null;
            return true;
        }

    }
}
