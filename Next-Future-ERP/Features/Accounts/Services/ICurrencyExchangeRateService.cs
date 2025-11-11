using Next_Future_ERP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface ICurrencyExchangeRateService
    {
        Task<List<CurrencyExchangeRate>> GetAllAsync();
        Task<CurrencyExchangeRate?> GetByIdAsync(int id);
        Task AddAsync(CurrencyExchangeRate model);
        Task UpdateAsync(CurrencyExchangeRate model);
        Task DeleteAsync(int id);
        Task<List<NextCurrency>> GetActiveCurrenciesAsync();
    }
}
