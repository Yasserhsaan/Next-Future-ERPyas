using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Data.Services;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class DatabaseConnectionViewModel : ObservableValidator, IWizardStepValidation
    {
        private readonly WizardState _wizardState;
        // Server name input
        [ObservableProperty]
        [Required(ErrorMessage = "أسم السرفر مطلوب")]
        private string? serverName;

        // Database name input
        [ObservableProperty]
        [Required(ErrorMessage = "أسم قاعدة البيانات مطلوب")]
        private string? dataBaseName;

        [ObservableProperty]
        private ObservableCollection<string> databases = new();


        // Windows or SQL Authentication
        [ObservableProperty]
        private bool useWindowsAuth = true;

        // Username for SQL Auth
        [ObservableProperty]
        private string? username;

        // Password for SQL Auth
        [ObservableProperty]
        private string? password;


        public DatabaseConnectionViewModel(WizardState wizardState)
        {

            _wizardState = wizardState;
            if(_wizardState.DatabaseConnection != null)
            {
                ServerName = _wizardState.DatabaseConnection.ServerName;
                DataBaseName = _wizardState.DatabaseConnection.DataBaseName;
                UseWindowsAuth = _wizardState.DatabaseConnection.UseWindowsAuth;
                Username = _wizardState.DatabaseConnection.Username;
                Password = _wizardState.DatabaseConnection.Password;
            }
            else
            {
                _wizardState.DatabaseConnection = new DatabaseConnectionModel();
            }

        }

        // Command to test connection
        [RelayCommand]
        private void TestConnection()
        {
           

            _wizardState.DatabaseConnection.ServerName = ServerName;
            _wizardState.DatabaseConnection.DataBaseName = DataBaseName;
            _wizardState.DatabaseConnection.UseWindowsAuth = UseWindowsAuth;
            _wizardState.DatabaseConnection.Username = Username;
            _wizardState.DatabaseConnection.Password = Password;



            SettingsService SettingsService = new SettingsService();
            SettingsService.Save(new ConnectionSettings
            {
                Server = ServerName ?? string.Empty,
                Database = DataBaseName ?? string.Empty,
                Type = _wizardState.SelectedConnectionType,
                Username = Username ?? string.Empty,
                Password = Password ?? string.Empty
            });

            DbContextFactory.TryConnect(out string? errorMessage);

            if (errorMessage != null)
            {
                MessageBox.Show($"فشل الاتصال بالخادم ❌\n\n{errorMessage}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                MessageBox.Show("تم الاتصال بنجاح ✅", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        

        }

        public bool ValidateStep(out string? errorMessage)
        {
            ValidateAllProperties();

            var errors = new[]
            {
                GetErrors(nameof(ServerName)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(DataBaseName)).Cast<ValidationResult>().FirstOrDefault()
            }
            .Where(e => e != null)
            .Select(e => e!.ErrorMessage)
            .ToList();

            if (errors.Any())
            {
                errorMessage = errors.First();
                return false;
            }

          
            _wizardState.DatabaseConnection.ServerName = ServerName ?? string.Empty;
            _wizardState.DatabaseConnection.DataBaseName = DataBaseName ?? string.Empty;
            _wizardState.DatabaseConnection.UseWindowsAuth = UseWindowsAuth ;
            _wizardState.DatabaseConnection.Username = Username ?? string.Empty;
            _wizardState.DatabaseConnection.Password = Password ?? string.Empty;
            _wizardState.DatabaseConnection.Type = _wizardState.SelectedConnectionType ?? string.Empty;


            errorMessage = null;
            return true;
        }
    }
}
