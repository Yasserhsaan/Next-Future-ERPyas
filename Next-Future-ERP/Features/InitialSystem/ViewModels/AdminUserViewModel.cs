using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class AdminUserViewModel : ObservableValidator, IWizardStepValidation
    {
        private readonly WizardState _wizardState;
        [ObservableProperty]
        [Required(ErrorMessage = "الكود مطلوب")]
        private string code;

        [ObservableProperty]
        [Required(ErrorMessage = "الاسم مطلوب")]
        private string name;

        [ObservableProperty]
        private string firstName;

        [ObservableProperty]
        [Phone]
        private string mobile;

        [ObservableProperty]
        [Phone]
        [Required(ErrorMessage = "رقم التلفون مطلوب")]
        private string phone;

        [ObservableProperty]
        private string address;

        [ObservableProperty]
        [EmailAddress]
        private string email;

        [ObservableProperty]
       // [Required(ErrorMessage = "كلمة السر مطلوبة")]
        private string password;


        public AdminUserViewModel(WizardState wizardState) { 
            _wizardState = wizardState;

            if (_wizardState.AdminUser != null) { 
                    Code = _wizardState.AdminUser.Code;
                    Name = _wizardState.AdminUser.Name;
                    FirstName = _wizardState.AdminUser.Fname;
                    Mobile = _wizardState.AdminUser.Mobile;
                    Phone = _wizardState.AdminUser.Phone;
                    Address = _wizardState.AdminUser.Address;
                    Email = _wizardState.AdminUser.Email;
                    Password = _wizardState.AdminUser.Password;
            }
            else
            {
                _wizardState.AdminUser = new Data.Models.Nextuser();
            }
        }

        [RelayCommand]
        private void SaveData()
        {
            ValidateAllProperties();
        }

        public bool ValidateStep(out string? errorMessage)
        {
            ValidateAllProperties();

            var errors = new[]
            {

                GetErrors(nameof(Code)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(Name)).Cast<ValidationResult>().FirstOrDefault(),
                GetErrors(nameof(Phone)).Cast<ValidationResult>().FirstOrDefault(),
              //  GetErrors(nameof(Password.Empty)).Cast<ValidationResult>().FirstOrDefault(),






            }
            .Where(e => e != null)
            .Select(e => e!.ErrorMessage)
            .ToList();

            if (errors.Any())
            {
                errorMessage = errors.First();
                return false;
            }


            // Update the wizard state with the current values

            _wizardState.AdminUser.Code = Code;
            _wizardState.AdminUser.Name = Name;
            _wizardState.AdminUser.Fname = FirstName;
            _wizardState.AdminUser.Mobile = Mobile;
            _wizardState.AdminUser.Phone = Phone;
            _wizardState.AdminUser.Address = Address;
            _wizardState.AdminUser.Email = Email;
            _wizardState.AdminUser.Password = Password;



            errorMessage = null;
            return true;
        }
    }
}
