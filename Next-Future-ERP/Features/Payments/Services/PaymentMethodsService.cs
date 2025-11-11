using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;

namespace Next_Future_ERP.Features.Payments.Services
{
    public interface IPaymentMethodsService
    {
        Task<List<PaymentMethod>> GetAllAsync(byte? typeFilter, string? search);
        Task<int> AddAsync(PaymentMethod dto);  // يرجع Method_ID
        Task UpdateAsync(PaymentMethod dto);
        Task DeleteAsync(int id);
    }

    public sealed class PaymentMethodsService : IPaymentMethodsService
    {
        private readonly AppDbContext _db;
        public PaymentMethodsService(AppDbContext db) => _db = db;

        public async Task<List<PaymentMethod>> GetAllAsync(byte? typeFilter, string? search)
        {
            var q = _db.PaymentMethods
                       .Include(x => x.Type)
                       .AsNoTracking()
                       .AsQueryable();

            if (typeFilter.HasValue)
                q = q.Where(x => x.PaymentTypeId == typeFilter.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x => x.MethodName.Contains(s) || x.GLAccount.Contains(s));
            }

            return await q.OrderBy(x => x.MethodName).ToListAsync();
        }

        public async Task<int> AddAsync(PaymentMethod dto)
        {
            Normalize(dto);

            // Identity: تجاهل أي قيمة مسبقة
            dto.MethodId = 0;

            _db.ChangeTracker.Clear();
            await _db.PaymentMethods.AddAsync(dto);
            await _db.SaveChangesAsync();
            return dto.MethodId;
        }

        public async Task UpdateAsync(PaymentMethod dto)
        {
            if (dto.MethodId <= 0)
                throw new InvalidOperationException("لا يمكن التعديل بدون Method_ID.");

            Normalize(dto);
            _db.ChangeTracker.Clear();

            var affected = await _db.PaymentMethods
                .Where(x => x.MethodId == dto.MethodId)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.MethodName, dto.MethodName)
                    .SetProperty(p => p.GLAccount, dto.GLAccount)
                    .SetProperty(p => p.PaymentTypeId, dto.PaymentTypeId)
                    .SetProperty(p => p.ProviderId, dto.ProviderId)
                    .SetProperty(p => p.RequiresApproval, dto.RequiresApproval)
                    .SetProperty(p => p.IsActive, dto.IsActive)
                    .SetProperty(p => p.SupportsSplit, dto.SupportsSplit)
                );

            if (affected == 0)
                throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) return;

            _db.ChangeTracker.Clear();
            await _db.PaymentMethods
                     .Where(x => x.MethodId == id)
                     .ExecuteDeleteAsync();
        }

        private static void Normalize(PaymentMethod dto)
        {
            dto.MethodName = (dto.MethodName ?? "").Trim();
            dto.GLAccount = (dto.GLAccount ?? "").Trim();

            if (dto.MethodName.Length == 0)
                throw new InvalidOperationException("اسم الطريقة (Method_Name) مطلوب.");
            if (dto.MethodName.Length > 100)
                dto.MethodName = dto.MethodName[..100];

            if (dto.GLAccount.Length == 0)
                throw new InvalidOperationException("حساب GL_Account مطلوب.");
            if (dto.GLAccount.Length > 50)
                dto.GLAccount = dto.GLAccount[..50];

            if (dto.PaymentTypeId is < 1 or > 255)
                throw new InvalidOperationException("نوع الدفع (Payment_Type) غير صالح.");

            // اجعل الحقول المنطقية الافتراضية أكثر وضوحًا عند الإضافة
            dto.IsActive ??= true;
            dto.RequiresApproval ??= false;
            dto.SupportsSplit ??= false;
        }
    }
}
