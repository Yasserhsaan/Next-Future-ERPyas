using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items; // For ItemPriceDto
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public class ItemPricesService : IItemPricesService
    {
        private readonly AppDbContext _db;
        public ItemPricesService(AppDbContext db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<List<ItemPriceDto>> GetAllAsync(
            string? search, int? priceType, int? priceLevel, int? method,
            bool isActiveOnly, DateTime? from, DateTime? to,
            int? itemId = null,
            CancellationToken ct = default)
        {
            var q =
                from p in _db.ItemPrices.AsNoTracking()
                join i in _db.Items.AsNoTracking() on p.ItemID equals i.ItemID
                join u0 in _db.Units.AsNoTracking() on p.UnitID equals u0.UnitID into ug
                from u in ug.DefaultIfEmpty()
                select new { p, i, u };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x => x.i.ItemCode.Contains(s) || x.i.ItemName.Contains(s));
            }
            if (priceType.HasValue) q = q.Where(x => x.p.PriceType == priceType.Value);
            if (priceLevel.HasValue) q = q.Where(x => x.p.PriceLevelId == priceLevel.Value);
            if (method.HasValue) q = q.Where(x => x.p.Method == method.Value);
            if (isActiveOnly) q = q.Where(x => x.p.IsActive);
            if (from.HasValue) q = q.Where(x => x.p.EffectiveFrom >= from.Value);
            if (to.HasValue) q = q.Where(x => x.p.EffectiveTo <= to.Value);
            if (itemId.HasValue && itemId.Value > 0) q = q.Where(x => x.p.ItemID == itemId.Value);

            var list = await q
                .OrderBy(x => x.i.ItemName)
                .Select(x => new ItemPriceDto
                {
                    PriceID = x.p.PriceID,
                    CompanyId = x.p.CompanyId,
                    BranchId = x.p.BranchId,
                    ItemID = x.p.ItemID,
                    ItemCode = x.i.ItemCode,
                    ItemName = x.i.ItemName,
                    UnitID = x.p.UnitID,
                    UnitName = x.u != null ? x.u.UnitName : "",
                    CurrencyId = x.p.CurrencyId,

                    PriceType = x.p.PriceType,
                    PriceLevelId = x.p.PriceLevelId,
                    Method = x.p.Method,

                    PriceAmount = x.p.PriceAmount,
                    PricePercent = x.p.PricePercent,
                    SellPrice = x.p.SellPrice,
                    EffectiveFrom = x.p.EffectiveFrom,
                    EffectiveTo = x.p.EffectiveTo,
                    IsActive = x.p.IsActive,
                    IsDefault = x.p.IsDefault,
                    CreatedAt = x.p.CreatedAt,
                    CreatedBy = x.p.CreatedBy,
                    ModifiedAt = x.p.ModifiedAt,
                    ModifiedBy = x.p.ModifiedBy
                })
                .ToListAsync(ct);

            // التسميات خارج الـ Expression Tree (كما هي)
            foreach (var d in list)
            {
                d.PriceTypeName = d.PriceType == 1 ? "آخر سعر شراء"
                                 : d.PriceType == 2 ? "أعلى سعر شراء"
                                 : d.PriceType == 3 ? "المتوسط" : "";

                d.PriceLevelName = d.PriceLevelId == 1 ? "تجزئة"
                                  : d.PriceLevelId == 2 ? "جملة"
                                  : d.PriceLevelId == 3 ? "افتراضي" : "";

                d.PriceMethodName = d.Method == 1 ? "يدوي"
                                   : d.Method == 2 ? "نسبة"
                                   : d.Method == 3 ? "تلقائي" : "";
            }

            return list;
        }

        public Task<ItemPrice?> GetByIdAsync(int priceId, CancellationToken ct = default)
            => _db.ItemPrices.AsNoTracking().FirstOrDefaultAsync(x => x.PriceID == priceId, ct);

        public async Task<int> AddAsync(ItemPrice p, CancellationToken ct = default)
        {
            Normalize(p, isNew: true);
            await _db.ItemPrices.AddAsync(p, ct);
            await _db.SaveChangesAsync(ct);
            return p.PriceID;
        }

        public async Task UpdateAsync(ItemPrice p, CancellationToken ct = default)
        {
            if (p.PriceID <= 0) throw new InvalidOperationException("لا يمكن التعديل بدون معرف.");

            Normalize(p, isNew: false);

            var affected = await _db.ItemPrices
                .Where(x => x.PriceID == p.PriceID)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(x => x.CompanyId, p.CompanyId)
                    .SetProperty(x => x.BranchId, p.BranchId)
                    .SetProperty(x => x.ItemID, p.ItemID)
                    .SetProperty(x => x.UnitID, p.UnitID)
                    .SetProperty(x => x.PriceLevelId, p.PriceLevelId)
                    .SetProperty(x => x.CurrencyId, p.CurrencyId)
                    .SetProperty(x => x.PriceType, p.PriceType)
                    .SetProperty(x => x.Method, p.Method)
                    .SetProperty(x => x.PriceAmount, p.PriceAmount)
                    .SetProperty(x => x.PricePercent, p.PricePercent)
                    .SetProperty(x => x.SellPrice, p.SellPrice)
                    .SetProperty(x => x.EffectiveFrom, p.EffectiveFrom)
                    .SetProperty(x => x.EffectiveTo, p.EffectiveTo)
                    .SetProperty(x => x.IsActive, p.IsActive)
                    .SetProperty(x => x.IsDefault, p.IsDefault)
                    .SetProperty(x => x.ModifiedAt, p.ModifiedAt)
                    .SetProperty(x => x.ModifiedBy, p.ModifiedBy),
                    ct);

            if (affected == 0) throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");
        }

        public Task DeleteAsync(int priceId, CancellationToken ct = default)
        {
            if (priceId <= 0) return Task.CompletedTask;
            return _db.ItemPrices.Where(x => x.PriceID == priceId).ExecuteDeleteAsync(ct);
        }

        public async Task<List<(int ItemID, string ItemDisplay)>> GetItemsLookupAsync(string? search = null, CancellationToken ct = default)
        {
            var q = _db.Items.AsNoTracking().Select(i => new { i.ItemID, i.ItemCode, i.ItemName });
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x => x.ItemCode.Contains(s) || x.ItemName.Contains(s));
            }
            var list = await q.OrderBy(x => x.ItemName).ToListAsync(ct);
            return list.Select(x => (x.ItemID, $"{x.ItemCode} - {x.ItemName}")).ToList();
        }

        private static void Normalize(ItemPrice p, bool isNew)
        {
            if (p.ItemID <= 0) throw new InvalidOperationException("اختر الصنف.");
            if (p.UnitID <= 0) throw new InvalidOperationException("اختر الوحدة.");

            // حساب سعر البيع
            if (p.Method == 1) // يدوي
                p.SellPrice = p.PriceAmount ?? 0m;
            else if (p.Method == 2) // نسبة
            {
                var baseVal = p.PriceAmount ?? 0m;
                var pct = (p.PricePercent ?? 0) / 100m;
                p.SellPrice = baseVal + (baseVal * pct);
            }

            if (isNew)
            {
                p.CreatedAt = DateTime.Now;
                p.CreatedBy = 1; // TODO: current user
            }
            else
            {
                p.ModifiedAt = DateTime.Now;
                p.ModifiedBy = 1; // TODO: current user
            }
        }
    }
}
