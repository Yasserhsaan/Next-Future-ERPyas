using Next_Future_ERP.Features.Warehouses.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public interface IValuationGroupService
    {
        Task<List<ValuationGroup>> GetAllAsync();
        Task<ValuationGroup?> GetByIdAsync(int id);
        Task AddAsync(ValuationGroup model);
        Task UpdateAsync(ValuationGroup model);
        Task DeleteAsync(int id);

        // Old methods from previous implementation
        Task<List<ValuationGroup>> GetListAsync(int companyId);
        Task<ValuationGroup?> GetAsync(int id);
        Task<int> SaveAsync(ValuationGroup vm);
        Task<ValuationGroupAccount> GetAccountsAsync(int valuationGroupId, int companyId);
        Task UpsertAccountsAsync(ValuationGroupAccount a);
    }
}
