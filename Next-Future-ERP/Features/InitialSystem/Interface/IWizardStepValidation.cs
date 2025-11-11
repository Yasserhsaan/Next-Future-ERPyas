using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.InitialSystem.Interface
{
    public interface IWizardStepValidation
    {
        bool ValidateStep(out string? errorMessage);
    }
}
