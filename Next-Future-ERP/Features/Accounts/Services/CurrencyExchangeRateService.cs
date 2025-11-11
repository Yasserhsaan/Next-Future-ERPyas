using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class CurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private readonly AppDbContext _db;

        public CurrencyExchangeRateService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CurrencyExchangeRate>> GetAllAsync()
        {
            return await _db.CurrencyExchangeRates
                .Include(x => x.Currency)
                .AsNoTracking()
                .OrderByDescending(x => x.DateExchangeStart)
                .ToListAsync();
        }

        public async Task<CurrencyExchangeRate?> GetByIdAsync(int id)
        {
            return await _db.CurrencyExchangeRates
                .Include(x => x.Currency)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<NextCurrency>> GetActiveCurrenciesAsync()
        {
            return await _db.NextCurrencies
                .AsNoTracking()
                .OrderBy(x => x.CurrencyNameAr)
                .ToListAsync();
        }

        public async Task AddAsync(CurrencyExchangeRate model)
        {
            Normalize(model);
            Validate(model);

            _db.CurrencyExchangeRates.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(CurrencyExchangeRate model)
        {
            Normalize(model);
            Validate(model);

            await _db.CurrencyExchangeRates
                .Where(x => x.Id == model.Id)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.CurrencyId, model.CurrencyId)
                    .SetProperty(p => p.ExchangeRate, model.ExchangeRate)
                    .SetProperty(p => p.DateExchangeStart, model.DateExchangeStart)
                    .SetProperty(p => p.DateExchangeEnd, model.DateExchangeEnd)
                    .SetProperty(p => p.Status, model.Status)
                );
        }

        public async Task DeleteAsync(int id)
        {
            await _db.CurrencyExchangeRates.Where(x => x.Id == id).ExecuteDeleteAsync();
        }

        private static void Normalize(CurrencyExchangeRate model)
        {
            if (model.DateExchangeStart == default)
                model.DateExchangeStart = DateTime.Today;
            
            if (model.Status == null)
                model.Status = true;
        }

        private static void Validate(CurrencyExchangeRate model)
        {
            if (model.CurrencyId <= 0)
                throw new InvalidOperationException("العملة مطلوبة.");
            if (model.ExchangeRate <= 0)
                throw new InvalidOperationException("سعر الصرف يجب أن يكون أكبر من صفر.");
            if (model.DateExchangeEnd.HasValue && model.DateExchangeEnd < model.DateExchangeStart)
                throw new InvalidOperationException("تاريخ انتهاء الصرف يجب أن يكون بعد تاريخ البدء.");
        }
    }
}
