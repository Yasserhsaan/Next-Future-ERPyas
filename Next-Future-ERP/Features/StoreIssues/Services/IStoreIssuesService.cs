using Next_Future_ERP.Features.StoreIssues.Models;

namespace Next_Future_ERP.Features.StoreIssues.Services
{
    public interface IStoreIssuesService
    {
        Task<IEnumerable<StoreIssue>> GetAllAsync();
        Task<IEnumerable<StoreIssue>> GetAllAsync(string? searchText);
        Task<StoreIssue?> GetByIdAsync(long id);
        Task<StoreIssue?> GetByIdWithDetailsAsync(long id);
        Task<long> AddAsync(StoreIssue issue);
        Task UpdateAsync(StoreIssue issue);
        Task DeleteAsync(long id);
        Task<bool> PostAsync(long id);
        Task<bool> CancelAsync(long id);
        Task<string> GenerateIssueNumberAsync(int companyId, int branchId);
        Task<bool> IsIssueNumberUniqueAsync(string issueNumber, int companyId, int branchId, long? excludeId = null);
        Task<IEnumerable<StoreIssueDetail>> GetDetailsAsync(long issueId);
        Task AddDetailAsync(StoreIssueDetail detail);
        Task UpdateDetailAsync(StoreIssueDetail detail);
        Task DeleteDetailAsync(long detailId);
        Task RecalculateTotalsAsync(long issueId);
        Task<Dictionary<string, object>> PreviewAccountingEntriesAsync(long issueId);
        Task<Dictionary<string, object>> GetInventoryInfoAsync(int warehouseId, int itemId, int? batchId = null);
    }
}
