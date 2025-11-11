using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Items.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Items.Services
{
    public class ItemCostsService : IItemCostsService
    {
        private readonly AppDbContext _db;
        public ItemCostsService(AppDbContext db) => _db = db;

        public async Task<ItemCost?> GetByItemIdAsync(int itemId, CancellationToken ct = default)
            => await _db.ItemCosts.AsNoTracking().FirstOrDefaultAsync(x => x.ItemID == itemId, ct);

        public async Task<int> UpsertAsync(ItemCost cost, CancellationToken ct = default)
        {
            var existing = await _db.ItemCosts.FirstOrDefaultAsync(x => x.ItemID == cost.ItemID, ct);
            if (existing == null)
            {
                // اضمن ملء الحقول غير القابلة للإلغاء وفق DDL
                cost.CostMethod = (cost.CostMethod ?? "A").Trim().Substring(0,1).ToUpperInvariant();
                cost.StandardCost = cost.StandardCost;
                cost.LastPurchaseCost = cost.LastPurchaseCost;
                cost.MovingAverageCost = cost.MovingAverageCost;
                cost.FIFOCost = cost.FIFOCost;
                await _db.ItemCosts.AddAsync(cost, ct);
                await _db.SaveChangesAsync(ct);
                return cost.CostID;
            }

            await _db.ItemCosts
                .Where(x => x.ItemID == cost.ItemID)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(x => x.CostMethod, cost.CostMethod)
                    .SetProperty(x => x.LastCost, cost.LastCost)
                    .SetProperty(x => x.AvgCost, cost.AvgCost)
                    .SetProperty(x => x.MinCost, cost.MinCost)
                    .SetProperty(x => x.MaxCost, cost.MaxCost)
                    .SetProperty(x => x.StandardCost, cost.StandardCost)
                    .SetProperty(x => x.LastPurchaseCost, cost.LastPurchaseCost)
                    .SetProperty(x => x.MovingAverageCost, cost.MovingAverageCost)
                    .SetProperty(x => x.FIFOCost, cost.FIFOCost)
                    .SetProperty(x => x.LastPurchaseDate, cost.LastPurchaseDate)
                    .SetProperty(x => x.LastUpdate, cost.LastUpdate)
                , ct);

            return existing.CostID;
        }
    }
}


