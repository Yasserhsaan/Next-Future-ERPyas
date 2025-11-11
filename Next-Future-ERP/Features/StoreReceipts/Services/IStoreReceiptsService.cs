using Next_Future_ERP.Features.StoreReceipts.Models;

namespace Next_Future_ERP.Features.StoreReceipts.Services
{
    public interface IStoreReceiptsService
    {
        Task<List<StoreReceipt>> GetAllAsync(string? searchText = null, int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<StoreReceipt?> GetByIdAsync(long id);
        Task<long> AddAsync(StoreReceipt receipt, IEnumerable<StoreReceiptDetailed> details);
        Task UpdateAsync(StoreReceipt receipt, IEnumerable<StoreReceiptDetailed> details);
        Task DeleteAsync(long id);
        Task<string> GenerateNextNumberAsync(int companyId, int branchId);
        Task<bool> PostAsync(long id, int userId);
        Task<bool> UnpostAsync(long id, int userId);
        Task<bool> ApproveAsync(long id, int userId);
    }
}
