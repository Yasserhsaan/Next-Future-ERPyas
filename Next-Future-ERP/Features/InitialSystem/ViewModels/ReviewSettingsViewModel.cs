using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using Next_Future_ERP.Data.Services;
using Next_Future_ERP.Data.Factories;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class ReviewSettingsViewModel : ObservableObject, IWizardStepValidation
    {
        private readonly WizardState _wizardState;

        public ReviewSettingsViewModel(WizardState wizardState)
        {
            _wizardState = wizardState;
        }

        public WizardState WizardState => _wizardState;

        [RelayCommand]
        private async Task EditSettings()
        {
            // Navigate back to first step to allow editing
            // This would be handled by the main wizard controller
            MessageBox.Show("سيتم العودة إلى الخطوة الأولى للتعديل", "تعديل الإعدادات", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task SaveSettings()
        {
            try
            {
                // Show loading indicator
                // await ShowLoadingAsync();

                // Validate all settings before saving
                if (!ValidateStep(out string? errorMessage))
                {
                    MessageBox.Show(errorMessage, "خطأ في التحقق", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Save all settings to database
                await SaveAllSettingsAsync();

                // Show success message
                MessageBox.Show("تم حفظ جميع الإعدادات بنجاح!", "تم الحفظ", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigate to main application or login
                // await NavigateToMainApplicationAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Hide loading indicator
                // await HideLoadingAsync();
            }
        }

        private async Task SaveAllSettingsAsync()
        {
            try
            {
                // 1. Save database connection settings
                var settingsService = new SettingsService();
                var connectionSettings = new Next_Future_ERP.Data.Models.ConnectionSettings
                {
                    Type = _wizardState.SelectedConnectionType,
                    Server = _wizardState.DatabaseConnection.ServerName,
                    Database = _wizardState.DatabaseConnection.DataBaseName,
                    Username = _wizardState.DatabaseConnection.Username,
                    Password = _wizardState.DatabaseConnection.Password
                };
              //  settingsService.Save(connectionSettings);

                // 2. Save company information
                var companyService = new CompanyService();
                await companyService.SaveCompanyInfoAsync(_wizardState.CompanyInfo);

                // 3. Save branch information
                var branchService = new BranchService();
                await branchService.SaveBranchAsync(_wizardState.Branches);

                // 4. Save accounting setup
                var accountingSetupService = new AccountingSetupService();
                await accountingSetupService.SaveAccountingSetupAsync(_wizardState.AccountingSetup);

                // 5. Save financial periods
                var financialPeriodsService = new FinancialPeriodsService();
                await financialPeriodsService.SaveFinancialPeriodsAsync(_wizardState.FinancialPeriods);

                // 6. Create admin user
                var userService = new UserService();
                await userService.SaveAdminUserAsync(_wizardState.AdminUser);

              
            }
            catch (Exception ex)
            {
                throw new Exception($"حدث خطأ أثناء حفظ الإعدادات: {ex.Message}", ex);
            }
        }

        public bool ValidateStep(out string? errorMessage)
        {
            errorMessage = null;

            // Validate connection settings
            if (string.IsNullOrEmpty(_wizardState.SelectedConnectionType))
            {
                errorMessage = "يرجى تحديد نوع الاتصال";
                return false;
            }

            if (string.IsNullOrEmpty(_wizardState.DatabaseConnection.ServerName))
            {
                errorMessage = "يرجى إدخال اسم خادم قاعدة البيانات";
                return false;
            }

            // Validate company information
            if (string.IsNullOrEmpty(_wizardState.CompanyInfo.CompName))
            {
                errorMessage = "يرجى إدخال اسم الشركة";
                return false;
            }

            if (string.IsNullOrEmpty(_wizardState.CompanyInfo.Currency))
            {
                errorMessage = "يرجى تحديد العملة";
                return false;
            }

            // Validate branch information
            if (string.IsNullOrEmpty(_wizardState.Branches.BranchName))
            {
                errorMessage = "يرجى إدخال اسم الفرع";
                return false;
            }

            // Validate accounting setup
            if (string.IsNullOrEmpty(_wizardState.AccountingSetup.DefaultCashAccount))
            {
                errorMessage = "يرجى تحديد الحساب النقدي الافتراضي";
                return false;
            }

            // Validate admin user
            if (string.IsNullOrEmpty(_wizardState.AdminUser.Name))
            {
                errorMessage = "يرجى إدخال اسم المستخدم المدير";
                return false;
            }

            if (string.IsNullOrEmpty(_wizardState.AdminUser.Fname))
            {
                errorMessage = "يرجى إدخال الاسم الكامل للمدير";
                return false;
            }

            return true;
        }
    }
} 