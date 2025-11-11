using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public class UnitsLookupService : IUnitsLookupService
    {
        private readonly IUnitsService _units;
        public UnitsLookupService(IUnitsService units) => _units = units;

        public async Task<List<UnitModel>> GetAllAsync()
        {
            var list = await _units.GetAllAsync();
            return list;
        }
    }
}
