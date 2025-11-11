using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next_Future_ERP.Features.Items.Models;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemSuppliersService
    {
        Task<List<ItemSupplier>> GetByItemAsync(int itemId, CancellationToken ct = default);
        Task<ItemSupplier?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> AddAsync(ItemSupplier model, CancellationToken ct = default);
        Task UpdateAsync(ItemSupplier model, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}


