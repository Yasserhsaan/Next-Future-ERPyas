using Next_Future_ERP.Features.Items.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemsService
    {
        Task<List<Item>> GetAllAsync(string? search = null);
        Task<Item?> GetByIdAsync(int id);

        Task<(int ItemID, string ItemCode)> AddAsync(Item m);   // يرجّع ItemID والكود المُولد
        Task<string> UpdateAsync(Item m);  // يرجّع الكود الجديد إذا تم تغيير النوع
        Task DeleteAsync(int id);

        Task<List<ItemUnit>> GetItemUnitsAsync(int itemId);
        Task SetItemUnitsAsync(int itemId, IEnumerable<ItemUnit> units);
    }
}
