using Next_Future_ERP.Features.Items.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IInventoryBalanceService
    {
        Task<List<InventoryBalance>> GetByItemAsync(int itemId);
        Task<List<InventoryBalance>> GetByWarehouseAsync(int warehouseId);
        Task<InventoryBalance> GetByItemAndWarehouseAsync(int itemId, int warehouseId);
        Task<InventoryBalance> GetByItemWarehouseAndBatchAsync(int itemId, int warehouseId, int? batchId);
        Task<InventoryBalance> AddAsync(InventoryBalance balance);
        Task<InventoryBalance> UpdateAsync(InventoryBalance balance);
        Task DeleteAsync(long balanceId);
        Task<List<InventoryBalance>> GetAllAsync();
    }
}
