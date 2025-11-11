using Next_Future_ERP.Features.Warehouses.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public interface IUnitsService
    {
        Task<List<UnitModel>> GetAllAsync(CancellationToken ct = default);
        Task<List<UnitModel>> GetByUnitTypeAsync(char unitType, CancellationToken ct = default);
        Task SaveAsync(UnitModel unit, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
