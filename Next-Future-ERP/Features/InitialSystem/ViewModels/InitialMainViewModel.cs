using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Features.InitialSystem.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class InitialMainViewModel : ObservableObject
    {
        private readonly WizardState _wizardState = new();
     



        public bool CanGoNext => CurrentStepIndex < Steps.Count - 1;
        public bool CanGoBack => CurrentStepIndex > 0;

        [ObservableProperty]
        private ObservableCollection<INavigationViewItem> menuItems;

        public ObservableCollection<WizardStep> Steps { get; } = new()
            {
                new WizardStep { Title = "نوع الاتصال", PageType = typeof(ConnectionTypePage) },
                new WizardStep { Title = "اتصال قاعدة البيانات", PageType = typeof(DatabaseConnectionPage) },
                new WizardStep { Title = "معلومات الشركة", PageType = typeof(CompanyInfoPage) },
                new WizardStep { Title = "معلومات الفرع", PageType = typeof(BranchesPage) },
                new WizardStep { Title = "الإعدادات المحاسبية", PageType = typeof(AccountingSetupPage) },
                new WizardStep { Title = "إعداد الفترة المالية", PageType = typeof(FinancialPeriodPage) },
                new WizardStep { Title = "إنشاء مستخدم", PageType = typeof(AdminUserPage) },
                new WizardStep { Title = "مراجعة الإعدادات", PageType = typeof(ReviewSettingsPage) },
            };

        [ObservableProperty]
        private int currentStepIndex;

        [ObservableProperty]
        private UserControl? currentPage;

        partial void OnCurrentStepIndexChanged(int value)
        {
            NavigateToStep(value);
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoBack));

            NextStepCommand.NotifyCanExecuteChanged();
            PreviousStepCommand.NotifyCanExecuteChanged();
        }

       


        public InitialMainViewModel()
        {
           
            CurrentStepIndex = 0;
            NavigateToStep(0);
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void NextStep()
        {
            if (CurrentPage?.DataContext is IWizardStepValidation validator)
            {
                if (!validator.ValidateStep(out string? error))
                {
                    // Show bottom error using Snackbar or dialog
                    System.Windows.MessageBox.Show(error ?? "الرجاء إدخال البيانات المطلوبة", "خطأ", System.Windows.MessageBoxButton.OK, MessageBoxImage.Warning);


                    return;
                }
            }
            CurrentStepIndex++;
        }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void PreviousStep()
        {
            CurrentStepIndex--;
        }

        private void NavigateToStep(int index)
        {
            var step = Steps[index];

            switch (step.PageType.Name)
            {
                case nameof(ConnectionTypePage):
                    CurrentPage = new ConnectionTypePage(new ConnectionTypeViewModel(_wizardState));
                    break;

                case nameof(DatabaseConnectionPage):
                    CurrentPage = new DatabaseConnectionPage(new DatabaseConnectionViewModel(_wizardState));
                    break;

                case nameof(CompanyInfoPage):
                    CurrentPage = new CompanyInfoPage(new CompanyInfoViewModel(_wizardState));
                    break;

                case nameof(BranchesPage):
                    CurrentPage = new BranchesPage(new BranchesViewModel(_wizardState));
                    break;

                case nameof(AccountingSetupPage):
                    CurrentPage = new AccountingSetupPage(new AccountingSetupViewModel(_wizardState));
                    break;

                case nameof(FinancialPeriodPage):
                    CurrentPage = new FinancialPeriodPage(new FinancialPeriodViewModel(_wizardState));
                    break;

                case nameof(AdminUserPage):
                    CurrentPage = new AdminUserPage(new AdminUserViewModel(_wizardState));
                    break;

                case nameof(ReviewSettingsPage):
                    CurrentPage = new ReviewSettingsPage(new ReviewSettingsViewModel(_wizardState));
                    break;

                default:
                    CurrentPage = null;
                    break;
            }
        }
    }
}
