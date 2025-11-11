using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Features.Warehouses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Services
{
    public class ValuationGroupService : IValuationGroupService
    {
        private readonly AppDbContext _context;
       
        public ValuationGroupService()
        {
            _context = DbContextFactory.Create();
        }

        public async Task<List<ValuationGroup>> GetAllAsync()
            => await _context.ValuationGroups
                .OrderBy(x => x.ValuationGroupCode)
                .AsNoTracking().ToListAsync();

        public async Task<ValuationGroup?> GetByIdAsync(int id)
            => await _context.ValuationGroups.FindAsync(id);

        public async Task AddAsync(ValuationGroup model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            
            model.CreatedDate = DateTime.Now;
            _context.ValuationGroups.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ValuationGroup model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            
            var existing = await _context.ValuationGroups.FirstOrDefaultAsync(x => x.ValuationGroupId == model.ValuationGroupId);
            if (existing == null)
                throw new InvalidOperationException($"ValuationGroup #{model.ValuationGroupId} غير موجود.");

            existing.ValuationGroupCode = model.ValuationGroupCode;
            existing.ValuationGroupName = model.ValuationGroupName;
            existing.Description = model.Description;
            existing.IsActive = model.IsActive;
            existing.CostCenterId = model.CostCenterId;
            existing.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _context.ValuationGroups.FindAsync(id);
            if (e == null) return;
            _context.ValuationGroups.Remove(e);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ValuationGroup>> GetListAsync(int companyId)
            => await _context.ValuationGroups
                .Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.ValuationGroupCode)
                .AsNoTracking().ToListAsync();

        public async Task<ValuationGroup?> GetAsync(int id)
            => await _context.ValuationGroups.FindAsync(id);

        public async Task<int> SaveAsync(ValuationGroup vm)
        {
            if (vm.ValuationGroupId == 0)
            {
                vm.CreatedDate = DateTime.Now;
                _context.ValuationGroups.Add(vm);
            }
            else
            {
                var e = await _context.ValuationGroups.FindAsync(vm.ValuationGroupId);
                if (e == null) throw new InvalidOperationException("ValuationGroup not found.");

                e.ValuationGroupCode = vm.ValuationGroupCode;
                e.ValuationGroupName = vm.ValuationGroupName;
                e.Description = vm.Description;
                e.IsActive = vm.IsActive;
                e.ModifiedDate = DateTime.Now;
                e.CostCenterId = vm.CostCenterId;
            }
            await _context.SaveChangesAsync();
            return vm.ValuationGroupId;
        }

        // العلاقة "الوهمية": سجل واحد في ValuationGroupAccounts لكل مجموعة + شركة
        public async Task<ValuationGroupAccount> GetAccountsAsync(int valuationGroupId, int companyId)
        {
            var acc = await _context.ValuationGroupAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ValuationGroup == valuationGroupId && x.CompanyId == companyId);

            return acc ?? new ValuationGroupAccount
            {
                ValuationGroup = valuationGroupId,
                CompanyId = companyId
            };
        }

        public async Task UpsertAccountsAsync(ValuationGroupAccount a)
        {
            if (a.ValuationGroupAccountsId == 0)
                _context.ValuationGroupAccounts.Add(a);
            else
                _context.ValuationGroupAccounts.Update(a);

            await _context.SaveChangesAsync();
        }
    }
}
