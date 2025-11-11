using Next_Future_ERP.Features.Items.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemCostsService
    {
        Task<ItemCost?> GetByItemIdAsync(int itemId, CancellationToken ct = default);
        Task<int> UpsertAsync(ItemCost cost, CancellationToken ct = default);
    }
}


