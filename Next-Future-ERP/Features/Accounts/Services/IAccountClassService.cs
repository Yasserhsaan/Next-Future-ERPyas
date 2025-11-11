using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next_Future_ERP.Models;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface IAccountClassService
    {
        Task<List<AccountClass>> GetAllAsync(string? search = null);
        Task<AccountClass?> GetByIdAsync(int id);
        Task<int> AddAsync(AccountClass entity);
        Task UpdateAsync(AccountClass entity);
        Task DeleteAsync(int id);
        Task<List<AccountCategoryOption>> GetAccountCategoryOptionsAsync(string? categoryKey = null);
    }
}
