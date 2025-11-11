using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public class CategoriesLookupService : ICategoriesLookupService
    {
        private readonly ICategoryService _cats;
        public CategoriesLookupService(ICategoryService cats) => _cats = cats;

        public async Task<List<CategoryModel>> GetAllAsync()
        {
            var list = await _cats.GetAllAsync();
            return list;
        }
    }
}
