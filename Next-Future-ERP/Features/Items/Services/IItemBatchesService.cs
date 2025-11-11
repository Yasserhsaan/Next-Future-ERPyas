using Next_Future_ERP.Features.Items.Models;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemBatchesService
    {
        Task<List<ItemBatch>> GetByItemAsync(int itemId, CancellationToken ct = default);
        Task<int> AddAsync(ItemBatch model, CancellationToken ct = default);
        Task UpdateAsync(ItemBatch model, CancellationToken ct = default);
        Task DeleteAsync(int batchId, CancellationToken ct = default);
    }
}


