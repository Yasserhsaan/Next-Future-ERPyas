using Next_Future_ERP.Features.Warehouses.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryModel>> GetAllAsync(CancellationToken ct = default);
        Task<List<CategoryModel>> GetParentCategoriesAsync(CancellationToken ct = default);
        Task SaveAsync(CategoryModel category, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
