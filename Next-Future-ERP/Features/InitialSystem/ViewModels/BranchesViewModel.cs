using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.InitialSystem.Interface;
using Next_Future_ERP.Features.InitialSystem.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace Next_Future_ERP.Features.InitialSystem.ViewModels;

public partial class BranchesViewModel : ObservableValidator, IWizardStepValidation
{
    private readonly WizardState _wizardState;



    public BranchesViewModel(WizardState wizardState)
    {
        _wizardState = wizardState;
        if (_wizardState.Branches != null)
        {
            BranchName = _wizardState.Branches.BranchName;
            Location = _wizardState.Branches.Location;
            IsActive = true; // Default value for IsActive
        }
        else
        {
            _wizardState.Branches = new BranchModel();
            IsActive = true; // Default value for IsActive


        }
    }



    [ObservableProperty]
    [Required(ErrorMessage  = "اسم الفرع مطلوب")]
    private string? branchName;

    [ObservableProperty]
    private string? location;

    [ObservableProperty]
    private bool isActive;



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



        _wizardState.Branches.BranchName = BranchName;
        _wizardState.Branches.Location = Location;
        _wizardState.Branches.BranchId = 1;
        _wizardState.Branches.ComiId = 1; 
        _wizardState.Branches.IsActive = true; 




        errorMessage = null;
        return true;
    }





}
