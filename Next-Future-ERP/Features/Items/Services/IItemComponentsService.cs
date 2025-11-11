using Next_Future_ERP.Features.Items.Models;

namespace Next_Future_ERP.Features.Items.Services
{
    public interface IItemComponentsService
    {
        Task<List<ItemComponent>> GetByParentItemAsync(int parentItemId);
        Task<int> AddAsync(ItemComponent component);
        Task UpdateAsync(ItemComponent component);
        Task DeleteAsync(int componentId);
    }
}
