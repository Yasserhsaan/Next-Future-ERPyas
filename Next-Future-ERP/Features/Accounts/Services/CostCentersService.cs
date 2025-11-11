using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class CostCentersService : ICostCentersService
    {
        private readonly AppDbContext _db;

        public CostCentersService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CostCenter>> GetAllAsync(string? searchText = null)
        {
            var query = _db.CostCenter.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim();
                query = query.Where(x => x.CostCenterName.Contains(search) || 
                                        (x.LinkedAccounts ?? "").Contains(search) ||
                                        (x.Classification ?? "").Contains(search));
            }

            return await query.OrderBy(x => x.CostCenterName).ToListAsync();
        }

        public async Task<CostCenter?> GetByIdAsync(int id)
        {
            return await _db.CostCenter.AsNoTracking().FirstOrDefaultAsync(x => x.CostCenterId == id);
        }

        public async Task<int> AddAsync(CostCenter model)
        {
            Normalize(model);
            Validate(model);

            _db.CostCenter.Add(model);
            await _db.SaveChangesAsync();
            return model.CostCenterId;
        }

        public async Task UpdateAsync(CostCenter model)
        {
            Normalize(model);
            Validate(model);

            await _db.CostCenter
                .Where(x => x.CostCenterId == model.CostCenterId)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.CostCenterName, model.CostCenterName)
                    .SetProperty(p => p.LinkedAccounts, model.LinkedAccounts)
                    .SetProperty(p => p.Classification, model.Classification)
                    .SetProperty(p => p.IsActive, model.IsActive)
                    .SetProperty(p => p.UpdatedAt, DateTime.Now)
                );
        }

        public async Task DeleteAsync(int id)
        {
            await _db.CostCenter.Where(x => x.CostCenterId == id).ExecuteDeleteAsync();
        }

        private static void Normalize(CostCenter model)
        {
            model.CostCenterName = model.CostCenterName?.Trim() ?? string.Empty;
            model.LinkedAccounts = model.LinkedAccounts?.Trim() ?? string.Empty;
            model.Classification = model.Classification?.Trim() ?? string.Empty;
            
            if (model.CreatedAt == default)
                model.CreatedAt = DateTime.Now;
        }

        private static void Validate(CostCenter model)
        {
            if (string.IsNullOrWhiteSpace(model.CostCenterName))
                throw new InvalidOperationException("اسم مركز التكلفة مطلوب.");
        }
    }
}
