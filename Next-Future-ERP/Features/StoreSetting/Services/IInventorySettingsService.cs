using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Next_Future_ERP.Features.StoreSetting.Models;

namespace Next_Future_ERP.Features.StoreSetting.Services
{
    public interface IInventorySettingsService
    {
        Task<InventorySetting?> LoadAsync(int companyId, int branchId);
        Task<InventorySetting> SaveAsync(InventorySetting settings);
        Task<InventorySetting> ResetDefaultsAsync(int companyId, int branchId);
    }
}
