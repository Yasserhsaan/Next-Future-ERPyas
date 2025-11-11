using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Warehouses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly AppDbContext _db;
        public WarehouseService(AppDbContext db) => _db = db;

        public async Task<List<Warehouse>> GetAllAsync(string? search = null, int skip = 0, int take = 200, CancellationToken ct = default)
        {
            IQueryable<Warehouse> q = _db.Warehouses.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                // بحث غير حساس لحالة الأحرف (يعتمد على Collation قاعدة البيانات أيضًا)
                var term = search.Trim();
                q = q.Where(x =>
                    (x.WarehouseCode ?? "").Contains(term) ||
                    (x.WarehouseName ?? "").Contains(term));
            }

            return await q
                .OrderBy(x => x.WarehouseName)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Warehouses.FirstOrDefaultAsync(x => x.WarehouseID == id, ct);

        public async Task<Warehouse> UpsertAsync(Warehouse model, int? userId = null, CancellationToken ct = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // إدراج
            if (model.WarehouseID == 0)
            {
                model.CreatedDate = DateTime.Now;
                if (userId.HasValue) model.CreatedBy = userId;
                _db.Warehouses.Add(model);
            }
            else
            {
                // تحديث آمن: اجلب الأصل وحدث الحقول المسموح بها فقط
                var existing = await _db.Warehouses.FirstOrDefaultAsync(x => x.WarehouseID == model.WarehouseID, ct);
                if (existing == null)
                    throw new InvalidOperationException($"Warehouse #{model.WarehouseID} غير موجود.");

                existing.WarehouseCode = model.WarehouseCode;
                existing.WarehouseName = model.WarehouseName;
                existing.Location = model.Location;

                existing.ManagerID = model.ManagerID;
                existing.CompanyId = model.CompanyId;
                existing.BranshId = model.BranshId;

                existing.AllowNegativeStock = model.AllowNegativeStock;
                existing.UseBins = model.UseBins;
                existing.IsDefault = model.IsDefault;
                existing.IsActive = model.IsActive;

                existing.WarehouseType = model.WarehouseType;

                existing.UseReciveTransctions = model.UseReciveTransctions;
                existing.UseIsuuseTransctions = model.UseIsuuseTransctions;
                existing.UseTransferTransctions = model.UseTransferTransctions;
                existing.UseReturnTransctions = model.UseReturnTransctions;
                existing.UseCountTransctions = model.UseCountTransctions;
                existing.UseSalesTansctions = model.UseSalesTansctions;

                existing.DefultCostCenter = model.DefultCostCenter;

                existing.ModifiedDate = DateTime.Now;
                if (userId.HasValue) existing.ModifiedBy = userId;
            }

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // في حال أضفت RowVersion لاحقًا
                throw new InvalidOperationException("تعارض تحديث، حاول إعادة تحميل السجل.", ex);
            }

            return model.WarehouseID == 0
                ? model // EF سيسند ID بعد الحفظ
                : await GetByIdAsync(model.WarehouseID, ct) ?? model;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var m = await _db.Warehouses.FirstOrDefaultAsync(x => x.WarehouseID == id, ct);
            if (m != null)
            {
                _db.Warehouses.Remove(m);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
