using Next_Future_ERP.Features.StoreIssues.Models;

namespace Next_Future_ERP.Features.StoreIssues.Services
{
    public interface IIssueDestinationsService
    {
        Task<IEnumerable<IssueDestination>> GetAllAsync();
        Task<IEnumerable<IssueDestination>> GetAllAsync(string? searchText);
        Task<IssueDestination?> GetByIdAsync(int id);
        Task<int> AddAsync(IssueDestination model);
        Task UpdateAsync(IssueDestination model);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int companyId, int branchId, string destinationCode);
        Task<string> GenerateNextCodeAsync(int companyId, int branchId);
        Task<IEnumerable<IssueDestination>> GetActiveAsync();
        Task<IEnumerable<IssueDestination>> GetByTypeAsync(char destinationType);
    }
}
