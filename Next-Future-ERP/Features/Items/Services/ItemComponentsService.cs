using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;


namespace Next_Future_ERP.Features.Items.Services
{
    public class ItemComponentsService : IItemComponentsService
    {
        private readonly AppDbContext _db;

        public ItemComponentsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ItemComponent>> GetByParentItemAsync(int parentItemId)
        {
            return await _db.ItemComponents
                .Where(x => x.ParentItemID == parentItemId)
                .Join(_db.Items, // Join with Items table for component name
                      component => component.ComponentItemID,
                      item => item.ItemID,
                      (component, item) => new ItemComponent
                      {
                          ItemComponentID = component.ItemComponentID,
                          ParentItemID = component.ParentItemID,
                          ComponentItemID = component.ComponentItemID,
                          UnitID = component.UnitID,
                          Quantity = component.Quantity,
                          CreatedDate = component.CreatedDate,
                          ModifiedDate = component.ModifiedDate,
                          CreatedBy = component.CreatedBy,
                          ModifiedBy = component.ModifiedBy,
                          ComponentItemName = item.ItemName // Populate ComponentItemName
                      })
                .Join(_db.Units, // Join with Units table for unit name
                      component => component.UnitID,
                      unit => unit.UnitID,
                      (component, unit) => new ItemComponent
                      {
                          ItemComponentID = component.ItemComponentID,
                          ParentItemID = component.ParentItemID,
                          ComponentItemID = component.ComponentItemID,
                          UnitID = component.UnitID,
                          Quantity = component.Quantity,
                          CreatedDate = component.CreatedDate,
                          ModifiedDate = component.ModifiedDate,
                          CreatedBy = component.CreatedBy,
                          ModifiedBy = component.ModifiedBy,
                          ComponentItemName = component.ComponentItemName,
                          UnitName = unit.UnitName // Populate UnitName
                      })
                .AsNoTracking()
                .OrderBy(x => x.ComponentItemName)
                .ToListAsync();
        }

        public async Task<int> AddAsync(ItemComponent component)
        {
            Normalize(component);
            _db.ItemComponents.Add(component);
            await _db.SaveChangesAsync();
            return component.ItemComponentID;
        }

        public async Task UpdateAsync(ItemComponent component)
        {
            Normalize(component);
            component.ModifiedDate = DateTime.Now;
            await _db.ItemComponents
                .Where(x => x.ItemComponentID == component.ItemComponentID)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.ComponentItemID, component.ComponentItemID)
                    .SetProperty(x => x.UnitID, component.UnitID)
                    .SetProperty(x => x.Quantity, component.Quantity)
                    .SetProperty(x => x.ModifiedDate, component.ModifiedDate)
                    .SetProperty(x => x.ModifiedBy, component.ModifiedBy));
        }

        public async Task DeleteAsync(int componentId)
        {
            await _db.ItemComponents
                .Where(x => x.ItemComponentID == componentId)
                .ExecuteDeleteAsync();
        }

        private void Normalize(ItemComponent component)
        {
            if (component.Quantity <= 0)
                component.Quantity = 0m;
        }
    }
}
