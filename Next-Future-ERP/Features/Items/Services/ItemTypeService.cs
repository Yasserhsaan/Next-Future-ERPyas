using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;
using System.Collections.Generic;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemTypeService
    {
        Task<List<ItemType>> GetAllAsync();
        Task<ItemType?> GetByCodeAsync(string itemTypeCode);
        Task<ItemType?> GetByIdAsync(int id);
        Task<string> GenerateNextItemCodeAsync(string itemTypeCode);
    }

    public class ItemTypeService : IItemTypeService
    {
        private readonly AppDbContext _context;

        public ItemTypeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ItemType>> GetAllAsync()
        {
            return await _context.ItemTypes
                .AsNoTracking()
                .OrderBy(x => x.ItemTypeNameAr)
                .ToListAsync();
        }

        public async Task<ItemType?> GetByCodeAsync(string itemTypeCode)
        {
            if (string.IsNullOrWhiteSpace(itemTypeCode))
                return null;

            return await _context.ItemTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemTypeCode == itemTypeCode);
        }

        public async Task<ItemType?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _context.ItemTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemTypeID == id);
        }

        public async Task<string> GenerateNextItemCodeAsync(string itemTypeCode)
        {
            if (string.IsNullOrWhiteSpace(itemTypeCode))
                throw new InvalidOperationException("ItemTypeCode مطلوب.");

            var itemType = await GetByCodeAsync(itemTypeCode.Trim().ToUpperInvariant());
            if (itemType == null)
                throw new InvalidOperationException($"نوع الصنف '{itemTypeCode}' غير موجود في جدول ItemTypes.");

            var prefix = itemType.ItemTypeCode;   // حرف النوع (S/C/W/G/F...)
            var prefixLen = prefix.Length;
            const int width = 6;

            // احصل على آخر رقم معروف لهذا النوع (بالصورة التقليدية prefix+6digits)
            var lastCode = await _context.Items.AsNoTracking()
                .Where(x => x.ItemType == prefix
                         && x.ItemCode.StartsWith(prefix)
                         && x.ItemCode.Length == prefixLen + width)
                .OrderByDescending(x => x.ItemCode) // أسرع طريقة بسيطة
                .Select(x => x.ItemCode)
                .FirstOrDefaultAsync();

            int lastNumber = itemType.RangeStart - 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var numberPart = lastCode.Substring(prefixLen, width);
                if (int.TryParse(numberPart, out var n))
                    lastNumber = n;
            }

            // جرّب الأرقام واحدًا تلو الآخر حتى تجد فراغًا
            for (int next = Math.Max(lastNumber + 1, itemType.RangeStart);
                 next <= itemType.RangeEnd;
                 next++)
            {
                string candidate = $"{prefix}{next:D6}";
                bool exists = await _context.Items.AsNoTracking()
                    .AnyAsync(x => x.ItemCode == candidate);

                if (!exists)
                    return candidate; // لقينا كودًا متاحًا
            }

            throw new InvalidOperationException(
                $"انتهى النطاق لنوع '{prefix}'. النطاق: {itemType.RangeStart}-{itemType.RangeEnd}");
        }


    }
}
