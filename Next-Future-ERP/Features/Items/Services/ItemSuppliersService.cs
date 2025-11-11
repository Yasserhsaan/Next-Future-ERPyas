using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;

namespace Next_Future_ERP.Features.Items.Services
{
    public class ItemSuppliersService : IItemSuppliersService
    {
        private readonly AppDbContext _db;
        public ItemSuppliersService(AppDbContext db) { _db = db; }

        public async Task<List<ItemSupplier>> GetByItemAsync(int itemId, CancellationToken ct = default)
        {
            return await _db.ItemSuppliers
                .Where(s => s.ItemID == itemId)
                .Join(_db.Suppliers, 
                      itemSupplier => itemSupplier.SupplierID,
                      supplier => supplier.SupplierID,
                      (itemSupplier, supplier) => new ItemSupplier
                      {
                          ItemSupplierID = itemSupplier.ItemSupplierID,
                          ItemID = itemSupplier.ItemID,
                          SupplierID = itemSupplier.SupplierID,
                          SupplierPrice = itemSupplier.SupplierPrice,
                          CurrencyCode = itemSupplier.CurrencyCode,
                          IsPrimarySupplier = itemSupplier.IsPrimarySupplier,
                          CreatedDate = itemSupplier.CreatedDate,
                          ModifiedDate = itemSupplier.ModifiedDate,
                          CreatedBy = itemSupplier.CreatedBy,
                          ModifiedBy = itemSupplier.ModifiedBy,
                          SupplierName = supplier.SupplierName
                      })
                .AsNoTracking()
                .OrderByDescending(s => s.IsPrimarySupplier)
                .ThenBy(s => s.ItemSupplierID)
                .ToListAsync(ct);
        }

        public async Task<ItemSupplier?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Set<ItemSupplier>().FindAsync(new object?[] { id }, ct);
        }

        public async Task<int> AddAsync(ItemSupplier model, CancellationToken ct = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await _db.Set<ItemSupplier>().AddAsync(model, ct);
            await _db.SaveChangesAsync(ct);
            return model.ItemSupplierID;
        }

        public async Task UpdateAsync(ItemSupplier model, CancellationToken ct = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await _db.Set<ItemSupplier>()
                .Where(x => x.ItemSupplierID == id)
                .ExecuteDeleteAsync(ct);
        }
    }
}


