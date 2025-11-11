using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels
{
    public partial class ConnectionTypeViewModel : ObservableValidator, IWizardStepValidation
    {
        private readonly WizardState _wizardState;

        public ConnectionTypeViewModel(WizardState wizardState)
        {
            _wizardState = wizardState;
           if(_wizardState.SelectedConnectionType != null)
            {
                SelectedConnectionType = _wizardState.SelectedConnectionType;
            }
        }

        [ObservableProperty]
        [Required(ErrorMessage = "يرجى اختيار نوع الاتصال")]
        private string selectedConnectionType;

        [RelayCommand]
        private void CardClick(string type)
        {
            SelectedConnectionType = type;
        }

        public bool ValidateStep(out string? errorMessage)
        {
            ValidateAllProperties();

          
            var errors = GetErrors(nameof(SelectedConnectionType))
                .Cast<ValidationResult>()
                .Select(e => e.ErrorMessage)
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            if (errors.Any())
            {
                errorMessage = errors.First();
                return false;
            }

            _wizardState.SelectedConnectionType = SelectedConnectionType!;
            errorMessage = null;
            return true;
        }
    }
}
