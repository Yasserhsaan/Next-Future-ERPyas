using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Items; // For ItemPriceDto
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemPricesService
    {
        Task<List<ItemPriceDto>> GetAllAsync(
            string? search, int? priceType, int? priceLevel, int? method,
            bool isActiveOnly, DateTime? from, DateTime? to,
            int? itemId = null,
            CancellationToken ct = default);

        Task<ItemPrice?> GetByIdAsync(int priceId, CancellationToken ct = default);
        Task<int> AddAsync(ItemPrice p, CancellationToken ct = default);
        Task UpdateAsync(ItemPrice p, CancellationToken ct = default);
        Task DeleteAsync(int priceId, CancellationToken ct = default);

        // Lookup للصنف في ComboBox
        Task<List<(int ItemID, string ItemDisplay)>> GetItemsLookupAsync(
            string? search = null, CancellationToken ct = default);
    }
}
