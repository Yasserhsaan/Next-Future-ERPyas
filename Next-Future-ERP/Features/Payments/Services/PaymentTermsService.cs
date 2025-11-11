using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Payments.Models;

namespace Next_Future_ERP.Features.Payments.Services
{
    public interface IPaymentTermsService
    {
        Task<List<PaymentTerm>> GetAllAsync(string? search = null);
        Task<PaymentTerm?> GetByIdAsync(int termId);
        Task<int> AddAsync(PaymentTerm dto);   // يرجع الـ ID
        Task UpdateAsync(PaymentTerm dto);
        Task DeleteAsync(int termId);
    }

    public sealed class PaymentTermsService : IPaymentTermsService
    {
        private readonly AppDbContext _db;
        public PaymentTermsService(AppDbContext db) => _db = db;

        public async Task<List<PaymentTerm>> GetAllAsync(string? search = null)
        {
            var q = _db.PaymentTerms.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x =>
                    x.TermCode.Contains(s) ||
                    x.TermName.Contains(s));
            }

            return await q.OrderBy(x => x.TermCode).ToListAsync();
        }

        public Task<PaymentTerm?> GetByIdAsync(int termId) =>
            _db.PaymentTerms.AsNoTracking().FirstOrDefaultAsync(x => x.TermId == termId);

        public async Task<int> AddAsync(PaymentTerm dto)
        {
            Normalize(dto);
            _db.ChangeTracker.Clear();

            try
            {
                await _db.PaymentTerms.AddAsync(dto);
                await _db.SaveChangesAsync();
                return dto.TermId; // Identity
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                if (msg.Contains("UQ_Payment_Terms_Code", StringComparison.OrdinalIgnoreCase)
                 || msg.Contains("Term_Code", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("رمز الشرط (Term_Code) مستخدم من قبل.", ex);

                throw;
            }
        }

        public async Task UpdateAsync(PaymentTerm dto)
        {
            Normalize(dto);
            _db.ChangeTracker.Clear();

            try
            {
                // تحديث بلا تتبّع (EF Core 7/8)
                var affected = await _db.PaymentTerms
                    .Where(x => x.TermId == dto.TermId)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(p => p.TermCode, dto.TermCode)
                        .SetProperty(p => p.TermName, dto.TermName)
                        .SetProperty(p => p.NetDays, dto.NetDays)
                        .SetProperty(p => p.DiscountPercent, dto.DiscountPercent)
                        .SetProperty(p => p.DiscountDays, dto.DiscountDays)
                        .SetProperty(p => p.LateFeePercent, dto.LateFeePercent)
                        .SetProperty(p => p.IsActive, dto.IsActive)
                    );

                if (affected == 0)
                    throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                if (msg.Contains("UQ_Payment_Terms_Code", StringComparison.OrdinalIgnoreCase)
                 || msg.Contains("Term_Code", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("رمز الشرط (Term_Code) مستخدم من قبل.", ex);

                throw;
            }
        }

        public async Task DeleteAsync(int termId)
        {
            _db.ChangeTracker.Clear();

            // حذف بلا تتبّع
            await _db.PaymentTerms
                     .Where(x => x.TermId == termId)
                     .ExecuteDeleteAsync();
        }

        private static void Normalize(PaymentTerm dto)
        {
            dto.TermCode = (dto.TermCode ?? string.Empty).Trim();
            dto.TermName = (dto.TermName ?? string.Empty).Trim();

            if (dto.TermCode.Length == 0) throw new InvalidOperationException("يجب إدخال Term_Code.");
            if (dto.TermCode.Length > 20) throw new InvalidOperationException("Term_Code بحد أقصى 20 حرف.");
            if (dto.TermName.Length == 0) throw new InvalidOperationException("يجب إدخال Term_Name.");
            if (dto.TermName.Length > 100) dto.TermName = dto.TermName[..100];
            if (dto.NetDays < 0) throw new InvalidOperationException("Net_Days لا بد أن يكون ≥ 0.");

            // قصّ آمن لِـ decimal(5,2)
            if (dto.DiscountPercent.HasValue)
                dto.DiscountPercent = Math.Round(dto.DiscountPercent.Value, 2, MidpointRounding.AwayFromZero);
            if (dto.LateFeePercent.HasValue)
                dto.LateFeePercent = Math.Round(dto.LateFeePercent.Value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
