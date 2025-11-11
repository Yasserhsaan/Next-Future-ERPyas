using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;
using System.Text.RegularExpressions;

namespace Next_Future_ERP.Features.Items.Services
{
    public sealed class ItemsService : IItemsService
    {
        private readonly AppDbContext _db;
        private readonly IItemTypeService _itemTypeService;

        public ItemsService(AppDbContext db, IItemTypeService itemTypeService)
        {
            _db = db;
            _itemTypeService = itemTypeService;
        }

        public async Task<List<Item>> GetAllAsync(string? search = null)
        {
            try
            {
                var q = _db.Items.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    q = q.Where(x =>
                        x.ItemCode.Contains(s) ||
                        x.ItemName.Contains(s) ||
                        (x.ItemBarcode ?? "").Contains(s));
                }

                // تضمين التصنيف والوحدة للعرض
                q = q.Include(x => x.Category)
                     .Include(x => x.Unit);

                return await q.OrderBy(x => x.ItemName).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطأ في GetAllAsync: {ex.Message}");
                throw new InvalidOperationException($"خطأ في جلب الأصناف: {ex.Message}", ex);
            }
        }

        public Task<Item?> GetByIdAsync(int id)
            => _db.Items.AsNoTracking().FirstOrDefaultAsync(x => x.ItemID == id);

        public async Task<(int ItemID, string ItemCode)> AddAsync(Item m)
        {
            // توليد/التحقق من الكود
            await NormalizeAsync(m, isNew: true);

            const int maxRetries = 3;
            int attempt = 0;

            while (true)
            {
                try
                {
                    // نفس المنطق السابق لكن على نفس DbContext
                    await _db.Items.AddAsync(m);
                    await _db.SaveChangesAsync();
                    return (m.ItemID, m.ItemCode);
                }
                catch (DbUpdateException ex)
                {
                    if (IsUniqueConstraintViolation(ex) && attempt < maxRetries)
                    {
                        attempt++;
                        // جرّب كود جديد ثم أعد المحاولة
                        m.ItemCode = await _itemTypeService.GenerateNextItemCodeAsync(m.ItemType);

                        // ملاحظة: الكيان ما زال مضافًا (Added)،
                        // تحديث الخاصية ثم SaveChanges محاولة ثانية كافٍ.
                        // لو رغبت بمحاكاة "سياق جديد" بالضبط:
                        // _db.ChangeTracker.Clear(); _db.Items.Add(m);
                        continue;
                    }
                    throw;
                }
            }
        }

        public async Task<string> UpdateAsync(Item m)
        {
            if (m.ItemID <= 0)
                throw new InvalidOperationException("لا يمكن التعديل بدون ItemID.");

            // جلب السجل الحالي
            var existing = await _db.Items.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemID == m.ItemID);
            if (existing == null)
                throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");

            string? newItemCode = null;

            // لو تغيّر نوع الصنف => كود جديد
            if (existing.ItemType != m.ItemType)
            {
                newItemCode = await _itemTypeService.GenerateNextItemCodeAsync(m.ItemType);
                m.ItemCode = newItemCode;
            }
            else
            {
                m.ItemCode = string.IsNullOrWhiteSpace(m.ItemCode) ? existing.ItemCode : m.ItemCode.Trim();
                if (string.IsNullOrEmpty(m.ItemCode))
                    throw new InvalidOperationException("ItemCode مطلوب.");
            }

            await NormalizeAsync(m, isNew: false);

            const int maxRetries = 3;
            int attempt = 0;
            while (true)
            {
                try
                {
                    var affected = await _db.Items
                        .Where(x => x.ItemID == m.ItemID)
                        .ExecuteUpdateAsync(set => set
                            .SetProperty(p => p.ItemCode, m.ItemCode)
                            .SetProperty(p => p.ItemName, m.ItemName)
                            .SetProperty(p => p.CategoryID, m.CategoryID)
                            .SetProperty(p => p.ItemBarcode, m.ItemBarcode)
                            .SetProperty(p => p.ItemType, m.ItemType)
                            .SetProperty(p => p.IsBatchTracked, m.IsBatchTracked)
                            .SetProperty(p => p.IsSerialTracked, m.IsSerialTracked)
                            .SetProperty(p => p.HasExpiryDate, m.HasExpiryDate)
                            .SetProperty(p => p.UnitID, m.UnitID)
                            .SetProperty(p => p.Weight, m.Weight)
                            .SetProperty(p => p.Volume, m.Volume)
                            .SetProperty(p => p.MinStockLevel, m.MinStockLevel)
                            .SetProperty(p => p.MaxStockLevel, m.MaxStockLevel)
                            .SetProperty(p => p.ReorderLevel, m.ReorderLevel)
                            .SetProperty(p => p.StandardCost, m.StandardCost)
                            .SetProperty(p => p.LastPurchasePrice, m.LastPurchasePrice)
                            .SetProperty(p => p.ValuationGroup, m.ValuationGroup)
                            .SetProperty(p => p.IsActive, m.IsActive)
                            .SetProperty(p => p.ModifiedDate, DateTime.Now)
                        );

                    if (affected == 0)
                        throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");

                    return newItemCode ?? m.ItemCode;
                }
                catch (DbUpdateException ex)
                {
                    if (IsUniqueConstraintViolation(ex) && attempt < maxRetries)
                    {
                        attempt++;
                        m.ItemCode = await _itemTypeService.GenerateNextItemCodeAsync(m.ItemType);
                        newItemCode = m.ItemCode;
                        continue;
                    }
                    throw;
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) return;

            _db.ChangeTracker.Clear();

            await _db.ItemUnits.Where(u => u.ItemID == id).ExecuteDeleteAsync();
            // TODO: احذف تبعيات أخرى عند الحاجة (أسعار، مكونات ...)

            await _db.Items.Where(s => s.ItemID == id).ExecuteDeleteAsync();
        }

        public Task<List<ItemUnit>> GetItemUnitsAsync(int itemId)
            => _db.ItemUnits
                .Where(x => x.ItemID == itemId)
                .Include(x => x.Unit)
                .AsNoTracking()
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Unit!.UnitName)
                .ToListAsync();

        public async Task SetItemUnitsAsync(int itemId, IEnumerable<ItemUnit> units)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _db.Database.BeginTransactionAsync();

                _db.ChangeTracker.Clear();

                // حذف الحالي
                var existingUnits = await _db.ItemUnits
                    .Where(x => x.ItemID == itemId)
                    .ToListAsync();

                if (existingUnits.Any())
                {
                    _db.ItemUnits.RemoveRange(existingUnits);
                    await _db.SaveChangesAsync();
                }

                var primary = units.FirstOrDefault(x => x.IsPrimary.HasValue && x.IsPrimary.Value);
                int? primaryUnitId = primary?.UnitID;

                var rows = units.Select(x => new ItemUnit
                {
                    ItemID = itemId,
                    UnitID = x.UnitID,
                    UnitBarcode = x.UnitBarcode,
                    BarcodeType = x.BarcodeType,
                    IsPrimary = primaryUnitId.HasValue && x.UnitID == primaryUnitId.Value,
                    IsSalesUnit = x.IsSalesUnit,
                    PurchaseUnit = x.PurchaseUnit,
                    IsInventoryUnit = x.IsInventoryUnit,
                    CreatedDate = DateTime.Now,
                    CreatedBy = x.CreatedBy
                }).ToList();

                if (rows.Count > 0)
                {
                    await _db.ItemUnits.AddRangeAsync(rows);
                    await _db.SaveChangesAsync();

                    if (!primaryUnitId.HasValue)
                        primaryUnitId = rows[0].UnitID;

                    await _db.Items.Where(i => i.ItemID == itemId)
                        .ExecuteUpdateAsync(set => set.SetProperty(p => p.UnitID, primaryUnitId!.Value));
                }

                await tx.CommitAsync();
            });
        }

        private async Task NormalizeAsync(Item m, bool isNew)
        {
            // ---- تنظيف ----
            m.ItemName = (m.ItemName ?? "").Trim();
            m.ItemBarcode = m.ItemBarcode?.Trim();
            m.ItemType = (m.ItemType ?? "S").Trim().ToUpperInvariant();

            if (m.ItemName.Length == 0)
                throw new InvalidOperationException("ItemName مطلوب.");

            // ---- نوع الصنف والنطاق ----
            var itemType = await _itemTypeService.GetByCodeAsync(m.ItemType);
            if (itemType == null)
                throw new InvalidOperationException($"نوع الصنف '{m.ItemType}' غير موجود في ItemTypes.");

            var prefix = itemType.ItemTypeCode;
            var pattern = $"^{Regex.Escape(prefix)}\\d{{6}}$";
            var re = new Regex(pattern, RegexOptions.CultureInvariant);

            if (isNew)
            {
                if (string.IsNullOrWhiteSpace(m.ItemCode))
                {
                    m.ItemCode = await _itemTypeService.GenerateNextItemCodeAsync(m.ItemType);
                }
                else
                {
                    m.ItemCode = m.ItemCode.Trim().ToUpperInvariant();

                    if (!re.IsMatch(m.ItemCode))
                        throw new InvalidOperationException(
                            $"صيغة ItemCode غير صحيحة. المتوقّع: {prefix} متبوعًا بـ 6 أرقام (مثل {prefix}000001).");

                    var numberPart = m.ItemCode.Substring(prefix.Length);
                    if (!int.TryParse(numberPart, out var num))
                        throw new InvalidOperationException("تعذّر قراءة الجزء الرقمي من ItemCode.");

                    if (num < itemType.RangeStart || num > itemType.RangeEnd)
                        throw new InvalidOperationException(
                            $"ItemCode خارج النطاق: {itemType.RangeStart} - {itemType.RangeEnd}.");
                }

                m.CreatedDate ??= DateTime.Now;
                m.IsActive ??= true;
            }
            else
            {
                m.ItemCode = (m.ItemCode ?? "").Trim().ToUpperInvariant();
                if (m.ItemCode.Length == 0)
                    throw new InvalidOperationException("ItemCode مطلوب.");

                if (!re.IsMatch(m.ItemCode))
                {
                    m.ItemCode = await _itemTypeService.GenerateNextItemCodeAsync(m.ItemType);
                }
                else
                {
                    var numberPart = m.ItemCode.Substring(prefix.Length);
                    if (!int.TryParse(numberPart, out var num))
                        throw new InvalidOperationException("تعذّر قراءة الجزء الرقمي من ItemCode.");

                    if (num < itemType.RangeStart || num > itemType.RangeEnd)
                        throw new InvalidOperationException(
                            $"ItemCode خارج النطاق: {itemType.RangeStart} - {itemType.RangeEnd}.");
                }
            }

            // ---- قيَم رقمية ----
            if (m.MinStockLevel is < 0) throw new InvalidOperationException("MinStockLevel لا يقبل السالب.");
            if (m.MaxStockLevel is < 0) throw new InvalidOperationException("MaxStockLevel لا يقبل السالب.");
            if (m.ReorderLevel is < 0) throw new InvalidOperationException("ReorderLevel لا يقبل السالب.");
            if (m.Weight is < 0) throw new InvalidOperationException("Weight لا يقبل السالب.");
            if (m.Volume is < 0) throw new InvalidOperationException("Volume لا يقبل السالب.");
            if (m.StandardCost is < 0) throw new InvalidOperationException("StandardCost لا يقبل السالب.");
            if (m.LastPurchasePrice is < 0) throw new InvalidOperationException("LastPurchasePrice لا يقبل السالب.");

            m.ModifiedDate = DateTime.Now;
        }

        private static bool IsUniqueConstraintViolation(Exception ex)
        {
            var message = ex.GetBaseException().Message;
            return message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("primary key", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Violation of UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("Violation of PRIMARY KEY", StringComparison.OrdinalIgnoreCase);
        }
    }
}
