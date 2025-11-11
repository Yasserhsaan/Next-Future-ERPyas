using Next_Future_ERP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface INextCurrencyService
    {
        Task<List<NextCurrency>> GetAllAsync();
        Task<NextCurrency?> GetByIdAsync(int id);
        Task AddAsync(NextCurrency model);
        Task UpdateAsync(NextCurrency model);
        Task DeleteAsync(int id);
    }
}
