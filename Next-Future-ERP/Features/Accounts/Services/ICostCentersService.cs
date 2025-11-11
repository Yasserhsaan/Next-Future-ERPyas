using Next_Future_ERP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface ICostCentersService
    {
        Task<List<CostCenter>> GetAllAsync(string? searchText = null);
        Task<CostCenter?> GetByIdAsync(int id);
        Task<int> AddAsync(CostCenter model);
        Task UpdateAsync(CostCenter model);
        Task DeleteAsync(int id);
    }
}
