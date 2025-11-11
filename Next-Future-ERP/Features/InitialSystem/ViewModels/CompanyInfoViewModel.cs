using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


public class SelectableItem
{
    public int Value { get; set; }
    public bool IsEnabled { get; set; } = true;
}

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class CompanyInfoViewModel : ObservableValidator, IWizardStepValidation
    {
        private readonly WizardState _wizardState;
        public ObservableCollection<SelectableItem> ListAccountNumbers { get; } = new();


        [ObservableProperty] 
        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        private string? companyName;


        [ObservableProperty] private BitmapImage? logoImage;

        [ObservableProperty] 
        [Required(ErrorMessage = "الرجاء اختيار اللغة")]
        private string selectedLanguage = "Arabic";
        [ObservableProperty] 
        [Required(ErrorMessage = "الرجاء اختيار العملة")]
        private string selectedCurrency = "Saudi Riyal (SAR)";
        [ObservableProperty] 
        [Required(ErrorMessage = "الرجاء اختيار مستوى الحساب الفرعي")]
        private int subAccountLevel = 0;
        [ObservableProperty] 
        [Required(ErrorMessage = "الرجاء اختيار السنة المالية")]
        private string fiscalYear = "Gregorian";
        [ObservableProperty]
        private SelectableItem? selectedAssetsStart;

        [ObservableProperty]
        private SelectableItem? selectedRevenueStart;

        [ObservableProperty]
        private SelectableItem? selectedLiabilitiesStart;

        [ObservableProperty]
        private SelectableItem? selectedExpenseStart;

        [ObservableProperty]
        private SelectableItem? selectedEquityStart;


        [ObservableProperty] private int minPasswordLength = 0;
        [ObservableProperty] private int accountNumberLength = 0;
        [ObservableProperty] private bool enableHijriCalendar;
        [ObservableProperty] private bool enableForeignCurrency;
        [ObservableProperty] private bool enableMultiDeviceLogin;
        [ObservableProperty] private bool useVat;
        [ObservableProperty] private bool enableCostCenters;

        public ObservableCollection<string> Languages { get; } = new() { "Arabic", "English" };
        public ObservableCollection<string> Currencies { get; } = new() { "Saudi Riyal ", "US Dollar ",  };
        public ObservableCollection<string> FiscalYears { get; } = new() 
        {
           ( DateTime.Now.Year - 1).ToString(),
           ( DateTime.Now.Year).ToString(),
           (DateTime.Now.Year + 1).ToString()

        };
        public ObservableCollection<int> SubAccountLevels { get; } = new() { 0, 1, 2, 3, 4 };


        public CompanyInfoViewModel(WizardState wizardState)

        {
            LoadAccountNumbers();
            _wizardState = wizardState;
            if(_wizardState.CompanyInfo != null)
            {
                CompanyName = _wizardState.CompanyInfo.CompName;
                LogoImage = _wizardState.CompanyInfo.LogoUrl;
                SelectedLanguage = _wizardState.CompanyInfo.Language;
                SelectedCurrency = _wizardState.CompanyInfo.Currency;
                SubAccountLevel = _wizardState.CompanyInfo.SubAccountLevel??0;
                FiscalYear = _wizardState.CompanyInfo.FiscalYearStart?? "";
                MinPasswordLength = _wizardState.CompanyInfo.MinPasswordLength ?? 0;
                AccountNumberLength = _wizardState.CompanyInfo.AccountNumberLength ?? 0;
                EnableHijriCalendar = _wizardState.CompanyInfo.HijriAdjustment == 1 ?true :false ;
                EnableForeignCurrency = _wizardState.CompanyInfo.EnableForeignCurrency?? false ;
                EnableMultiDeviceLogin = _wizardState.CompanyInfo.MultiDeviceLogin ?? false;
                UseVat = _wizardState.CompanyInfo.UseVat ?? false;
                EnableCostCenters = _wizardState.CompanyInfo.EnableCostCenters ?? false;
                SelectedAssetsStart.Value = _wizardState.CompanyInfo.AssetsStart ?? 1;
                SelectedRevenueStart.Value = _wizardState.CompanyInfo.RevenueStart ?? 2;
                SelectedLiabilitiesStart.Value = _wizardState.CompanyInfo.LiabilitiesStart ?? 3;
                SelectedExpenseStart.Value = _wizardState.CompanyInfo.ExpenseStart ?? 4;
                SelectedEquityStart.Value = _wizardState.CompanyInfo.EquityStart ?? 5;

            }
            else
            {
                _wizardState.CompanyInfo = new CompanyInfoModel();
                SelectedAssetsStart.Value = 1;
                SelectedRevenueStart.Value = 2;
                SelectedLiabilitiesStart.Value = 3;
                SelectedExpenseStart.Value = 4;
                SelectedEquityStart.Value = 5;
                RefreshList();
            }
        }

        [RelayCommand]
        private void ChooseImage()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select Logo",
                Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg"
            };

            if (dlg.ShowDialog() == true)
            {
                var img = new BitmapImage(new System.Uri(dlg.FileName));
                LogoImage = img;
            }
        }


        public bool ValidateStep(out string? errorMessage)
        {
            ValidateAllProperties();

            var errors = new[]
            {
                GetErrors(nameof(CompanyName)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(LogoImage)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedLanguage)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedCurrency)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SubAccountLevel)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(FiscalYear)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedAssetsStart.Value)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedRevenueStart.Value)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedLiabilitiesStart.Value)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedExpenseStart.Value)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(SelectedEquityStart.Value)).Cast<ValidationResult>().FirstOrDefault(),


            }
            .Where(e => e != null)
            .Select(e => e!.ErrorMessage)
            .ToList();

            if (errors.Any())
            {
                errorMessage = errors.First();
                return false;
            }


           
            _wizardState.CompanyInfo.CompName = CompanyName;
            _wizardState.CompanyInfo.LogoUrl = LogoImage;
            _wizardState.CompanyInfo.Language = SelectedLanguage;
            _wizardState.CompanyInfo.Currency = SelectedCurrency;
            _wizardState.CompanyInfo.SubAccountLevel = SubAccountLevel;
            _wizardState.CompanyInfo.FiscalYearStart = FiscalYear;
            _wizardState.CompanyInfo.MinPasswordLength = MinPasswordLength;
            _wizardState.CompanyInfo.AccountNumberLength = AccountNumberLength;
            _wizardState.CompanyInfo.HijriAdjustment = EnableHijriCalendar ? 1 : 0;
            _wizardState.CompanyInfo.EnableForeignCurrency = EnableForeignCurrency; 
            _wizardState.CompanyInfo.MultiDeviceLogin = EnableMultiDeviceLogin;
            _wizardState.CompanyInfo.UseVat = UseVat;
            _wizardState.CompanyInfo.EnableCostCenters = EnableCostCenters;
            _wizardState.CompanyInfo.AssetsStart = SelectedAssetsStart?.Value;
            _wizardState.CompanyInfo.RevenueStart = SelectedRevenueStart?.Value;
            _wizardState.CompanyInfo.LiabilitiesStart = SelectedLiabilitiesStart?.Value;
            _wizardState.CompanyInfo.ExpenseStart = SelectedExpenseStart?.Value;
            _wizardState.CompanyInfo.EquityStart = SelectedEquityStart?.Value;



            errorMessage = null;
            return true;
        }

        private void LoadAccountNumbers()
        {
            ListAccountNumbers.Clear();

            for (int i = 1; i <= 5; i++)
                ListAccountNumbers.Add(new SelectableItem { Value = i });

            SelectedAssetsStart = ListAccountNumbers.First(x => x.Value == 1);
            SelectedRevenueStart = ListAccountNumbers.First(x => x.Value == 2);
            SelectedLiabilitiesStart = ListAccountNumbers.First(x => x.Value == 3);
            SelectedExpenseStart = ListAccountNumbers.First(x => x.Value == 4);
            SelectedEquityStart = ListAccountNumbers.First(x => x.Value == 5);

            RefreshList();
        }

        private void RefreshList()
        {
            var selected = new List<int?>
            {
                SelectedAssetsStart?.Value,
                SelectedRevenueStart?.Value,
                SelectedLiabilitiesStart?.Value,
                SelectedExpenseStart?.Value,
                SelectedEquityStart?.Value
            };

            foreach (var item in ListAccountNumbers)
                item.IsEnabled = !selected.Contains(item.Value);
        }


        partial void OnSelectedAssetsStartChanged(SelectableItem? value) => RefreshList();
        partial void OnSelectedRevenueStartChanged(SelectableItem? value) => RefreshList();
        partial void OnSelectedLiabilitiesStartChanged(SelectableItem? value) => RefreshList();
        partial void OnSelectedExpenseStartChanged(SelectableItem? value) => RefreshList();
        partial void OnSelectedEquityStartChanged(SelectableItem? value) => RefreshList();
    }
}
