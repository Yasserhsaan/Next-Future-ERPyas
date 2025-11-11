using Next_Future_ERP.Features.Warehouses.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IUnitsLookupService
    {
        Task<List<UnitModel>> GetAllAsync();
    }
}
