using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Suppliers.Models;

namespace Next_Future_ERP.Features.Suppliers.Services
{
    public interface ISuppliersService
    {
        Task<List<Supplier>> GetAllAsync(string? search = null);
        Task<Supplier?> GetByIdAsync(int id);

        Task<int> AddAsync(Supplier s);        // يعيد ID
        Task UpdateAsync(Supplier s);
        Task DeleteAsync(int id);

        Task<List<SupplierPaymentMethod>> GetSupplierPaymentMethodsAsync(int supplierId);
        Task SetSupplierPaymentMethodsAsync(int supplierId, IEnumerable<SupplierPaymentMethod> methods);
    }

    public sealed class SuppliersService : ISuppliersService
    {
        private readonly AppDbContext _db;
        public SuppliersService(AppDbContext db) => _db = db;

        public async Task<List<Supplier>> GetAllAsync(string? search = null)
        {
            var q = _db.Suppliers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x =>
                    x.SupplierCode.Contains(s) ||
                    x.SupplierName.Contains(s) ||
                    (x.TaxNumber ?? "").Contains(s));
            }

            return await q.OrderBy(x => x.SupplierName).ToListAsync();
        }

        public Task<Supplier?> GetByIdAsync(int id) =>
            _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(x => x.SupplierID == id);

        public async Task<int> AddAsync(Supplier s)
        {
            Normalize(s, isNew: true);
            _db.ChangeTracker.Clear();

            await _db.Suppliers.AddAsync(s);
            await _db.SaveChangesAsync();  // Identity
            return s.SupplierID;
        }

        public async Task UpdateAsync(Supplier s)
        {
            if (s.SupplierID <= 0)
                throw new InvalidOperationException("لا يمكن التعديل بدون معرف المورد.");

            Normalize(s, isNew: false);
            _db.ChangeTracker.Clear();

            var affected = await _db.Suppliers
                .Where(x => x.SupplierID == s.SupplierID)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.SupplierCode, s.SupplierCode)
                    .SetProperty(p => p.SupplierName, s.SupplierName)
                    .SetProperty(p => p.TaxNumber, s.TaxNumber)
                    .SetProperty(p => p.AccountID, s.AccountID)
                    .SetProperty(p => p.CostCenterID, s.CostCenterID)
                    .SetProperty(p => p.PaymentTerms, s.PaymentTerms)
                    .SetProperty(p => p.CreditLimit, s.CreditLimit)
                    .SetProperty(p => p.ContactPerson, s.ContactPerson)
                    .SetProperty(p => p.Phone, s.Phone)
                    .SetProperty(p => p.Email, s.Email)
                    .SetProperty(p => p.Address, s.Address)
                    .SetProperty(p => p.IsActive, s.IsActive)
                    .SetProperty(p => p.ModifiedDate, DateTime.Now)
                    .SetProperty(p => p.Nationality, s.Nationality)
                    .SetProperty(p => p.IDNumber, s.IDNumber)
                    .SetProperty(p => p.CRNumber, s.CRNumber)
                    .SetProperty(p => p.VATNumber, s.VATNumber)
                    .SetProperty(p => p.DefaultPaymentMethodID, s.DefaultPaymentMethodID)
                );

            if (affected == 0)
                throw new InvalidOperationException("السجل المطلوب تعديله غير موجود.");
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) return;

            _db.ChangeTracker.Clear();

            // أولًا: حذف طرق الدفع المرتبطة (Composite PK)
            await _db.SupplierPaymentMethods
                     .Where(pm => pm.SupplierID == id)
                     .ExecuteDeleteAsync();

            // ثم حذف المورد
            await _db.Suppliers
                     .Where(s => s.SupplierID == id)
                     .ExecuteDeleteAsync();
        }

        public async Task<List<SupplierPaymentMethod>> GetSupplierPaymentMethodsAsync(int supplierId)
        {
            return await _db.SupplierPaymentMethods
                .Where(x => x.SupplierID == supplierId)
                .Include(x => x.Method) // مهم
                .AsNoTracking()
                .OrderByDescending(x => x.Is_Default)
                .ThenBy(x => x.Method!.MethodName)
                .ToListAsync();
        }
         

        public async Task SetSupplierPaymentMethodsAsync(int supplierId, IEnumerable<SupplierPaymentMethod> methods)
        {
            await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                _db.ChangeTracker.Clear();

                using var tx = await _db.Database.BeginTransactionAsync();

                // 1) احذف الموجود
                await _db.SupplierPaymentMethods
                         .Where(x => x.SupplierID == supplierId)
                         .ExecuteDeleteAsync();

                // 2) ثبّت الافتراضي (واحد فقط)
                var oneDefault = methods.FirstOrDefault(m => m.Is_Default);
                int? defaultMethodId = oneDefault?.Method_ID;

                var rows = methods.Select(m => new SupplierPaymentMethod
                {
                    SupplierID = supplierId,
                    Method_ID = m.Method_ID,
                    Is_Default = (defaultMethodId.HasValue && m.Method_ID == defaultMethodId.Value)
                }).ToList();

                if (rows.Count > 0)
                {
                    // 3) أضف الجديد
                    await _db.SupplierPaymentMethods.AddRangeAsync(rows);

                    // 👈 مهم: خزّن التغييرات
                    await _db.SaveChangesAsync();

                    // 4) عكس الافتراضي على جدول Suppliers
                    await _db.Suppliers
                        .Where(s => s.SupplierID == supplierId)
                        .ExecuteUpdateAsync(set => set
                            .SetProperty(p => p.DefaultPaymentMethodID, defaultMethodId)
                        );
                }
                else
                {
                    // لا توجد طرق — صفّر الافتراضي
                    await _db.Suppliers
                        .Where(s => s.SupplierID == supplierId)
                        .ExecuteUpdateAsync(set => set
                            .SetProperty(p => p.DefaultPaymentMethodID, (int?)null)
                        );
                }

                // 5) أنهِ الترانزاكشن
                await tx.CommitAsync();
            });
        }

        private static void Normalize(Supplier s, bool isNew)
        {
            s.SupplierCode = (s.SupplierCode ?? "").Trim();
            s.SupplierName = (s.SupplierName ?? "").Trim();
            s.TaxNumber = (s.TaxNumber ?? "").Trim();
            s.ContactPerson = s.ContactPerson?.Trim();
            s.Phone = s.Phone?.Trim();
            s.Email = s.Email?.Trim();
            s.Address = s.Address?.Trim();
            s.Nationality = s.Nationality?.Trim();
            s.IDNumber = s.IDNumber?.Trim();
            s.CRNumber = s.CRNumber?.Trim();
            s.VATNumber = s.VATNumber?.Trim();

            if (s.SupplierCode.Length == 0) throw new InvalidOperationException("SupplierCode مطلوب.");
            if (s.SupplierName.Length == 0) throw new InvalidOperationException("SupplierName مطلوب.");
            if (s.TaxNumber.Length == 0) throw new InvalidOperationException("TaxNumber مطلوب.");
            if (s.AccountID <= 0) throw new InvalidOperationException("AccountID غير صالح.");

            if (isNew)
            {
                s.CreatedDate ??= DateTime.Now;
                s.IsActive ??= true;
            }
            s.ModifiedDate = DateTime.Now;
        }
    }
}
