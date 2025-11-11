// PosSettingService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Models;
using System;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Sales.Services
{
    public class PosSettingService : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public PosSettingService()
        {
            _context = DbContextFactory.Create();
            _ownsContext = true;
        }

        public PosSettingService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

        // PosSettingService.cs
        // PosSettingService.cs
        public async Task<PosSetting> GetByCompanyAndBranchAsync(int companyId, int branchId)
        {
            var setting = await _context.PosSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.BranchId == branchId);

            return setting ?? new PosSetting
            {
                CompanyId = companyId,
                BranchId = branchId,
                EnableEInvoice = false,
                QrCodeOnInvoice = false,
                ShowVatBreakdown = false,
                PrintInvoiceAuto = false,
                SearchForItems = true
            };
        }

        public async Task SaveAsync(PosSetting setting)
        {
            try
            {
                if (setting.PosSettingId > 0)
                {
                    var existing = await _context.PosSettings.FindAsync(setting.PosSettingId);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(setting);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    _context.PosSettings.Add(setting);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء حفظ إعدادات نقاط البيع", ex);
            }
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