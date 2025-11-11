using Next_Future_ERP.Features.Warehouses.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetAllAsync(string? search = null, int skip = 0, int take = 200, CancellationToken ct = default);
        Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Warehouse> UpsertAsync(Warehouse model, int? userId = null, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
