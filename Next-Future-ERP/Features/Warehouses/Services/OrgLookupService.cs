using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Warehouses.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public interface IOrgLookupService
    {
        Task<List<LookupItem>> GetCompaniesAsync();
        Task<List<LookupItem>> GetBranchesByCompanyAsync(int companyId);
        Task<List<LookupItem>> GetInventoryCostCentersAsync();
    }

    public class OrgLookupService : IOrgLookupService
    {
        private readonly AppDbContext _db;
        public OrgLookupService(AppDbContext db) => _db = db;

        public async Task<List<LookupItem>> GetCompaniesAsync()
        {
            var list = await _db.CompanyInfo.AsNoTracking().OrderBy(c => c.CompName).ToListAsync();
            return list.Select(c => new LookupItem { Id = c.CompId, Name = c.CompName }).ToList();
        }

        public async Task<List<LookupItem>> GetBranchesByCompanyAsync(int companyId)
        {
            var list = await _db.Branches.AsNoTracking()
                .Where(b => b.ComiId == companyId)
                .OrderBy(b => b.BranchName)
                .ToListAsync();
            return list.Select(b => new LookupItem { Id = b.BranchId, Name = b.BranchName }).ToList();
        }

        public async Task<List<LookupItem>> GetInventoryCostCentersAsync()
        {
            // نفترض أن الجدول يحتوي عمود Classification أو CategoryKey يحمل نوع المركز
            var list = await _db.CostCenter.AsNoTracking()
                .Where(c => c.Classification == "inventory")
                .OrderBy(c => c.CostCenterName)
                .ToListAsync();
            return list.Select(c => new LookupItem { Id = c.CostCenterId, Name = c.CostCenterName }).ToList();
        }
    }
}


