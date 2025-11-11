using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Payments.Models;

namespace Next_Future_ERP.Features.Payments.Services
{
    public interface IPaymentTypesService
    {
        Task<List<PaymentType>> GetAllAsync(string? search = null);
        Task<PaymentType?> GetByIdAsync(byte typeId);
        Task<byte> AddAsync(PaymentType dto);   // ← يرجع Identity
        Task UpdateAsync(PaymentType dto);
        Task DeleteAsync(byte typeId);
    }

    public sealed class PaymentTypesService : IPaymentTypesService
    {
        private readonly AppDbContext _db;
        public PaymentTypesService(AppDbContext db) => _db = db;

        public async Task<List<PaymentType>> GetAllAsync(string? search = null)
        {
            IQueryable<PaymentType> q = _db.PaymentTypes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                var sUpper = s.ToUpperInvariant(); // Code = حرفان
                q = q.Where(x => x.Code == sUpper || (x.Description ?? "").Contains(s));
            }

            return await q.OrderBy(x => x.TypeId).ToListAsync();
        }

        public Task<PaymentType?> GetByIdAsync(byte typeId) =>
            _db.PaymentTypes.AsNoTracking().FirstOrDefaultAsync(x => x.TypeId == typeId);

        public async Task<byte> AddAsync(PaymentType dto)
        {
            Normalize(dto);

            // للإضافات على Identity، تجاهل أي قيمة مسبقة
            dto.TypeId = 0;

            _db.ChangeTracker.Clear();
            await _db.PaymentTypes.AddAsync(dto);
            await _db.SaveChangesAsync();

            return dto.TypeId; // Identity tinyint
        }

        public async Task UpdateAsync(PaymentType dto)
        {
            if (dto.TypeId == 0)
                throw new InvalidOperationException("لا يمكن التعديل بدون معرف (TypeId).");

            Normalize(dto);

            _db.ChangeTracker.Clear();

            // تحديث بلا تتبّع (EF Core 7/8)
            var affected = await _db.PaymentTypes
                .Where(x => x.TypeId == dto.TypeId)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.Code, dto.Code)
                    .SetProperty(p => p.Description, dto.Description)
                );

            if (affected == 0)
                throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");
        }

        public async Task DeleteAsync(byte typeId)
        {
            if (typeId == 0) return;

            _db.ChangeTracker.Clear();
            await _db.PaymentTypes
                     .Where(x => x.TypeId == typeId)
                     .ExecuteDeleteAsync();
        }

        private static void Normalize(PaymentType dto)
        {
            // char(2)
            dto.Code = (dto.Code ?? string.Empty).Trim().ToUpperInvariant();
            if (dto.Code.Length != 2)
                throw new InvalidOperationException("Code يجب أن يكون حرفين بالضبط.");

            // nvarchar(50) قصّ آمن
            if (dto.Description is { Length: > 50 })
                dto.Description = dto.Description[..50];
        }
    }
}
