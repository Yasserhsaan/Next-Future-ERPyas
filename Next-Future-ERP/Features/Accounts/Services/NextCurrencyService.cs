using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class NextCurrencyService : INextCurrencyService
    {
        private readonly AppDbContext _db;

        public NextCurrencyService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<NextCurrency>> GetAllAsync()
        {
            return await _db.NextCurrencies.AsNoTracking().OrderBy(x => x.CurrencyNameAr).ToListAsync();
        }

        public async Task<NextCurrency?> GetByIdAsync(int id)
        {
            return await _db.NextCurrencies.AsNoTracking().FirstOrDefaultAsync(x => x.CurrencyId == id);
        }

        public async Task AddAsync(NextCurrency model)
        {
            Normalize(model);
            Validate(model);

            _db.NextCurrencies.Add(model);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(NextCurrency model)
        {
            Normalize(model);
            Validate(model);

            await _db.NextCurrencies
                .Where(x => x.CurrencyId == model.CurrencyId)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.CurrencyNameAr, model.CurrencyNameAr)
                    .SetProperty(p => p.CurrencyNameEn, model.CurrencyNameEn)
                    .SetProperty(p => p.CurrencySymbol, model.CurrencySymbol)
                    .SetProperty(p => p.FractionUnit, model.FractionUnit)
                    .SetProperty(p => p.DecimalPlaces, model.DecimalPlaces)
                    .SetProperty(p => p.IsCompanyCurrency, model.IsCompanyCurrency)
                    .SetProperty(p => p.IsForeignCurrency, model.IsForeignCurrency)
                    .SetProperty(p => p.ExchangeRate, model.ExchangeRate)
                    .SetProperty(p => p.MinExchangeRate, model.MinExchangeRate)
                    .SetProperty(p => p.MaxExchangeRate, model.MaxExchangeRate)
                    .SetProperty(p => p.UpdatedAt, DateTime.Now)
                );
        }

        public async Task DeleteAsync(int id)
        {
            await _db.NextCurrencies.Where(x => x.CurrencyId == id).ExecuteDeleteAsync();
        }

        private static void Normalize(NextCurrency model)
        {
            model.CurrencyNameAr = model.CurrencyNameAr?.Trim() ?? string.Empty;
            model.CurrencyNameEn = model.CurrencyNameEn?.Trim() ?? string.Empty;
            model.CurrencySymbol = model.CurrencySymbol?.Trim() ?? string.Empty;
            model.FractionUnit = model.FractionUnit?.Trim() ?? string.Empty;
            
            if (model.CreatedAt == default)
                model.CreatedAt = DateTime.Now;
        }

        private static void Validate(NextCurrency model)
        {
            if (string.IsNullOrWhiteSpace(model.CurrencyNameAr))
                throw new InvalidOperationException("الاسم العربي مطلوب.");
            if (string.IsNullOrWhiteSpace(model.CurrencyNameEn))
                throw new InvalidOperationException("الاسم الإنجليزي مطلوب.");
            if (string.IsNullOrWhiteSpace(model.CurrencySymbol))
                throw new InvalidOperationException("رمز العملة مطلوب.");
        }
    }
}
