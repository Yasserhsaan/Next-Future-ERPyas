using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;

namespace Next_Future_ERP.Features.Items.Services
{
    public class ItemBatchesService : IItemBatchesService
    {
        private readonly AppDbContext _db;
        public ItemBatchesService(AppDbContext db) { _db = db; }

        public async Task<List<ItemBatch>> GetByItemAsync(int itemId, CancellationToken ct = default)
        {
            return await _db.ItemBatches
                .Where(b => b.ItemID == itemId)
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync(ct);
        }

        public async Task<int> AddAsync(ItemBatch model, CancellationToken ct = default)
        {
            await _db.ItemBatches.AddAsync(model, ct);
            await _db.SaveChangesAsync(ct);
            return model.BatchID;
        }

        public async Task UpdateAsync(ItemBatch model, CancellationToken ct = default)
        {
            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int batchId, CancellationToken ct = default)
        {
            await _db.ItemBatches.Where(b => b.BatchID == batchId).ExecuteDeleteAsync(ct);
        }
    }
}


