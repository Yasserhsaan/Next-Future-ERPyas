// SalesSettingService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Models;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Sales.Services
{
    public class SalesSettingService : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public SalesSettingService()
        {
            _context = DbContextFactory.Create();
            _ownsContext = true;
        }

        public SalesSettingService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

      

        public async Task SaveAsync(SalesSetting setting)
        {
            if (setting.SalesSettingId > 0)
            {
                var existing = await _context.SalesSettings.FindAsync(setting.SalesSettingId);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(setting);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                _context.SalesSettings.Add(setting);
                await _context.SaveChangesAsync();
            }
        }
        // InventorySettingService.cs
        // SalesSettingService.cs
        public async Task<SalesSetting> GetByCompanyAndBranchAsync(int companyId, int branchId)
        {
            var setting = await _context.SalesSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.BranchId == branchId);

            return setting ?? new SalesSetting
            {
                CompanyId = companyId,
                BranchId = branchId,
                AutoPostInvoice = false,
                AllowDiscount = false,
                PosEnabled = false,
                PosAutoPrint = false,
                DefaultTaxRate = 0,
                MaxDiscount = 0
            };
        }
        public void Dispose()
        {
            if (_ownsContext)
            {
                _context?.Dispose();
            }
        }
    }
}