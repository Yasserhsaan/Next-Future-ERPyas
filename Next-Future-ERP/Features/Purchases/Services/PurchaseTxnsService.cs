// Features/Purchases/Services/IPurchaseTxnsService.cs
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Purchases.Models;

namespace Next_Future_ERP.Features.Purchases.Services
{
  
    public sealed class PurchaseTxnsService : IPurchaseTxnsService
    {
        private readonly AppDbContext _db;
        public PurchaseTxnsService(AppDbContext db) => _db = db;

        public async Task<List<PurchaseTxn>> GetAllAsync(char txnType, string? q = null, int? supplierId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.Set<PurchaseTxn>().AsNoTracking().Where(x => x.TxnType == txnType);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim();
                query = query.Where(x => x.TxnNumber.Contains(s) || (x.Remarks ?? "").Contains(s));
            }
            if (supplierId.HasValue) query = query.Where(x => x.SupplierID == supplierId.Value);
            if (from.HasValue) query = query.Where(x => x.TxnDate >= from.Value);
            if (to.HasValue) query = query.Where(x => x.TxnDate <= to.Value);

            var results = await query
                .OrderByDescending(x => x.TxnDate)
                .ThenByDescending(x => x.TxnID)
                .ToListAsync();

            // تحميل بيانات الموردين والوصف
            foreach (var txn in results)
            {
                // تحميل اسم المورد
                var supplier = await _db.Suppliers
                    .Where(s => s.SupplierID == txn.SupplierID)
                    .Select(s => new { s.SupplierName })
                    .FirstOrDefaultAsync();
                
                txn.SupplierName = supplier?.SupplierName;
                
                // استخدام الملاحظات كوصف
                txn.Description = txn.Remarks;
            }

            return results;
        }
        public async Task<List<PurchaseTxn>> GetApprovedOrdersAsync()
        {
            var results = await _db.PurchaseTxns
                .Where(x => x.TxnType == 'P' && x.Status == 2) // 2 = معتمد
                .Include(x => x.Details)
                .AsNoTracking()
                .ToListAsync();

            // تحميل بيانات الموردين والوصف
            foreach (var txn in results)
            {
                // تحميل اسم المورد
                var supplier = await _db.Suppliers
                    .Where(s => s.SupplierID == txn.SupplierID)
                    .Select(s => new { s.SupplierName })
                    .FirstOrDefaultAsync();
                txn.SupplierName = supplier?.SupplierName;   
                // استخدام الملاحظات كوصف
                txn.Description = txn.Remarks;
            }

            return results;
        }
        public async Task<PurchaseTxn?> GetByIdAsync(int id)
        {
            return await _db.PurchaseTxns
                .Include(h => h.Details)           // ✅ ضروري
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.TxnID == id);
        }
     
        public async Task<int> AddAsync(PurchaseTxn head, IEnumerable<PurchaseTxnDetail> details)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.AddAsync: Starting add operation");
                
                await NormalizeAsync(head);
                Validate(head, details);
                CalcTotals(head, details);

            // استخدام Execution Strategy للتعامل مع المعاملات
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    await _db.Set<PurchaseTxn>().AddAsync(head);
                    await _db.SaveChangesAsync(); // يحدد TxnID

                    var rows = new List<PurchaseTxnDetail>();
                    foreach (var d in details)
                    {
                        rows.Add(PrepareDetail(d, head));
                    }

                    await _db.Set<PurchaseTxnDetail>().AddRangeAsync(rows);
                    await _db.SaveChangesAsync();

                    await tx.CommitAsync();
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.AddAsync: Successfully added transaction with ID: {head.TxnID}");
                    return head.TxnID;
                }
                catch (DbUpdateException ex)
                {
                    await tx.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.AddAsync: Database update error: {ex.Message}");
                    var root = ex.GetBaseException().Message;
                    throw new InvalidOperationException(
                        $"فشل الحفظ: {root}", ex);
                }
            });
            }
            catch (InvalidOperationException)
            {
                // إعادة رمي استثناءات التحقق كما هي
                System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.AddAsync: Validation error occurred");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.AddAsync: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ غير متوقع أثناء إضافة المستند: {ex.Message}", ex);
            }
        }
        public async Task<string> GenerateNextNumberAsync(int companyId, int branchId, char txnType)
        {
            // صيغة: P-2025-00001 أو R-2025-00001
            var prefix = txnType == 'R' ? "R" : "P";
            var year = DateTime.Today.Year;

            // ابحث آخر رقم موجود لنفس الشركة/الفرع/النوع
            var last = await _db.PurchaseTxns
                .Where(x => x.CompanyID == companyId && x.BranchID == branchId && x.TxnType == txnType)
                .OrderByDescending(x => x.TxnID)
                .Select(x => x.TxnNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrWhiteSpace(last))
            {
                // حاول تقرأ الجزء الأخير (بعد الشرطة الأخيرة)
                var parts = last.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[^1], out var seq))
                    nextSeq = seq + 1;
            }

            return $"{prefix}-{year}-{nextSeq:D5}";
        }


        private static void Validate(PurchaseTxn head, IEnumerable<PurchaseTxnDetail> details)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.Validate: Starting validation");
                
                // التحقق من البيانات الأساسية
                if (head == null)
                {
                    System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.Validate: head is null");
                    throw new InvalidOperationException("بيانات المستند مطلوبة.");
                }

                if (head.CompanyID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Invalid CompanyID: {head.CompanyID}");
                    throw new InvalidOperationException("معرف الشركة مطلوب.");
                }

                if (head.BranchID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Invalid BranchID: {head.BranchID}");
                    throw new InvalidOperationException("معرف الفرع مطلوب.");
                }

                if (head.TxnType != 'P' && head.TxnType != 'R')
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Invalid TxnType: {head.TxnType}");
                    throw new InvalidOperationException("نوع المستند يجب أن يكون أمر شراء (P) أو مرتجع (R).");
                }

                if (string.IsNullOrWhiteSpace(head.TxnNumber))
                {
                    System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.Validate: TxnNumber is null or empty");
                    throw new InvalidOperationException("رقم المستند مطلوب.");
                }

                if (head.TxnNumber.Length > 50)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: TxnNumber too long: {head.TxnNumber.Length}");
                    throw new InvalidOperationException("رقم المستند يتجاوز 50 حرفاً.");
                }

                if (head.SupplierID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Invalid SupplierID: {head.SupplierID}");
                    throw new InvalidOperationException("المورد مطلوب.");
                }

                if (head.TxnDate == default)
                {
                    System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.Validate: TxnDate is default");
                    throw new InvalidOperationException("تاريخ المستند مطلوب.");
                }

                // التحقق من التفاصيل
                var list = details?.ToList() ?? [];
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Details count: {list.Count}");
                
                if (list.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.Validate: No details found");
                    throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل.");
                }

                // التحقق من كل سطر
                for (int i = 0; i < list.Count; i++)
                {
                    var d = list[i];
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Validating detail {i + 1}");

                    if (d == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Detail {i + 1} is null");
                        throw new InvalidOperationException($"السطر {i + 1}: بيانات السطر مطلوبة.");
                    }

                    if (d.ItemID <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Detail {i + 1} - Invalid ItemID: {d.ItemID}");
                        throw new InvalidOperationException($"السطر {i + 1}: الصنف مطلوب.");
                    }

                    if (d.UnitID <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Detail {i + 1} - Invalid UnitID: {d.UnitID}");
                        throw new InvalidOperationException($"السطر {i + 1}: الوحدة مطلوبة.");
                    }

                    if (d.Quantity <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Detail {i + 1} - Invalid Quantity: {d.Quantity}");
                        throw new InvalidOperationException($"السطر {i + 1}: الكمية يجب أن تكون أكبر من صفر.");
                    }

                    if (d.UnitPrice < 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Detail {i + 1} - Invalid UnitPrice: {d.UnitPrice}");
                        throw new InvalidOperationException($"السطر {i + 1}: سعر الوحدة لا يقبل القيم السالبة.");
                    }

                    if (d.VATRate < 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Detail {i + 1} - Invalid VATRate: {d.VATRate}");
                        throw new InvalidOperationException($"السطر {i + 1}: نسبة الضريبة لا تقبل القيم السالبة.");
                    }
                }

                System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.Validate: Validation completed successfully");
            }
            catch (InvalidOperationException)
            {
                // إعادة رمي الاستثناءات الصحيحة كما هي
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.Validate: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ في التحقق من صحة البيانات: {ex.Message}");
            }
        }

        public async Task UpdateAsync(PurchaseTxn head, IEnumerable<PurchaseTxnDetail> details)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.UpdateAsync: Starting update operation");
                
                if (head.TxnID <= 0) 
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.UpdateAsync: Invalid TxnID: {head.TxnID}");
                    throw new InvalidOperationException("لا يمكن التعديل بدون معرف المستند.");
                }

                await NormalizeAsync(head);
                CalcTotals(head, details);

            // استخدام Execution Strategy للتعامل مع المعاملات
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    // تحديث الرأس
                    await _db.Set<PurchaseTxn>()
                        .Where(x => x.TxnID == head.TxnID)
                        .ExecuteUpdateAsync(set => set
                            .SetProperty(p => p.SupplierID, head.SupplierID)
                            .SetProperty(p => p.TxnDate, head.TxnDate)
                            .SetProperty(p => p.ExpectedDelivery, head.ExpectedDelivery)
                            .SetProperty(p => p.Status, head.Status)
                            .SetProperty(p => p.SubTotal, head.SubTotal)
                            .SetProperty(p => p.TaxAmount, head.TaxAmount)
                            .SetProperty(p => p.TotalAmount, head.TotalAmount)
                            .SetProperty(p => p.Remarks, head.Remarks)
                            .SetProperty(p => p.ModifiedAt, DateTime.Now)
                            .SetProperty(p => p.ModifiedBy, head.ModifiedBy)
                        );

                    // استبدال تفاصيل
                    await _db.Set<PurchaseTxnDetail>().Where(d => d.TxnID == head.TxnID).ExecuteDeleteAsync();
                    var preparedDetails = details.Select(d => PrepareDetail(d, head)).ToList();
                    await _db.Set<PurchaseTxnDetail>().AddRangeAsync(preparedDetails);
                    await _db.SaveChangesAsync();

                    await tx.CommitAsync();
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.UpdateAsync: Successfully updated transaction with ID: {head.TxnID}");
                }
                catch (DbUpdateException ex)
                {
                    await tx.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.UpdateAsync: Database update error: {ex.Message}");
                    var root = ex.GetBaseException().Message;
                    throw new InvalidOperationException(
                        $"فشل التحديث: {root}", ex);
                }
            });
            }
            catch (InvalidOperationException)
            {
                // إعادة رمي استثناءات التحقق كما هي
                System.Diagnostics.Debug.WriteLine("PurchaseTxnsService.UpdateAsync: Validation error occurred");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.UpdateAsync: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ غير متوقع أثناء تحديث المستند: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            // استخدام Execution Strategy للتعامل مع المعاملات
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    await _db.Set<PurchaseTxnDetail>().Where(d => d.TxnID == id).ExecuteDeleteAsync();
                    await _db.Set<PurchaseTxn>().Where(h => h.TxnID == id).ExecuteDeleteAsync();
                    await tx.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await tx.RollbackAsync();
                    var root = ex.GetBaseException().Message;
                    throw new InvalidOperationException(
                        $"فشل الحذف: {root}", ex);
                }
            });
        }

        private async Task NormalizeAsync(PurchaseTxn head)
        {
            if (head.CompanyID == 0) head.CompanyID = 1;
            if (head.BranchID == 0) head.BranchID = 1;

            if (string.IsNullOrWhiteSpace(head.TxnNumber))
                head.TxnNumber = await GenerateNextNumberAsync(head.CompanyID, head.BranchID, head.TxnType);

            if (head.TxnDate == default) head.TxnDate = DateTime.Today;
        }


        private static PurchaseTxnDetail PrepareDetail(PurchaseTxnDetail src, PurchaseTxn head)
        {
            var d = new PurchaseTxnDetail
            {
                TxnID = head.TxnID,                // FK
                CompanyID = head.CompanyID,            // NOT NULL
                BranchID = head.BranchID,             // NOT NULL

                ItemID = src.ItemID,                // NOT NULL
                UnitID = src.UnitID,                // NOT NULL
                Quantity = src.Quantity <= 0 ? 1 : Math.Round(src.Quantity, 4),
                UnitPrice = Math.Round(src.UnitPrice, 4),

                VATRate = Math.Round(src.VATRate, 2),// NOT NULL (بـ DEFAULT 0 لكن عبيها)
                                                     // القيم المحسوبة:
                TaxableAmount = 0, // سنحسبها تحت
                VATAmount = 0,
                LineTotal = 0,

                ReceivedQuantity = head.TxnType == 'P'
                                   ? (src.ReceivedQuantity ?? 0)
                                   : null,                 // مرتجع غالباً لا يستخدم المستلم
                IsClosed = false,
                IsSynced = false
            };

            d.TaxableAmount = Math.Round(d.Quantity * d.UnitPrice, 4);
            d.VATAmount = Math.Round(d.TaxableAmount * (d.VATRate / 100m), 4);
            d.LineTotal = Math.Round(d.TaxableAmount + d.VATAmount, 4);

            return d;
        }

        private static void CalcTotals(PurchaseTxn head, IEnumerable<PurchaseTxnDetail> details)
        {
            var sub = details.Sum(d => Math.Round(d.Quantity * d.UnitPrice, 4));
            var tax = details.Sum(d => Math.Round((d.Quantity * d.UnitPrice) * (d.VATRate / 100m), 4));
            head.SubTotal = sub;
            head.TaxAmount = tax;
            head.TotalAmount = Math.Round(sub + tax, 4);
        }

        // تغيير حالة المستند
        public async Task<bool> ChangeStatusAsync(int txnId, byte newStatus)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.ChangeStatusAsync: Changing status for TxnID {txnId} to {newStatus}");
                
                var affectedRows = await _db.Set<PurchaseTxn>()
                    .Where(x => x.TxnID == txnId)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(p => p.Status, newStatus)
                        .SetProperty(p => p.ModifiedAt, DateTime.Now)
                    );

                var success = affectedRows > 0;
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.ChangeStatusAsync: Status change {(success ? "succeeded" : "failed")} - affected rows: {affectedRows}");
                
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.ChangeStatusAsync: Error changing status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PostAsync(int txnId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.PostAsync: Posting transaction {txnId}");
                
                // التحقق من أن الحالة الحالية هي مسودة (0)
                var currentStatus = await _db.Set<PurchaseTxn>()
                    .Where(x => x.TxnID == txnId)
                    .Select(x => x.Status)
                    .FirstOrDefaultAsync();

                if (currentStatus != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.PostAsync: Cannot post transaction {txnId} - current status is {currentStatus}, expected 0");
                    return false;
                }

                return await ChangeStatusAsync(txnId, 1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.PostAsync: Error posting transaction: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ApproveAsync(int txnId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.ApproveAsync: Approving transaction {txnId}");
                
                // التحقق من أن الحالة الحالية هي مرحل (1)
                var currentStatus = await _db.Set<PurchaseTxn>()
                    .Where(x => x.TxnID == txnId)
                    .Select(x => x.Status)
                    .FirstOrDefaultAsync();

                if (currentStatus != 1)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.ApproveAsync: Cannot approve transaction {txnId} - current status is {currentStatus}, expected 1");
                    return false;
                }

                return await ChangeStatusAsync(txnId, 2);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.ApproveAsync: Error approving transaction: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CancelAsync(int txnId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.CancelAsync: Cancelling transaction {txnId}");
                
                // التحقق من أن الحالة الحالية ليست ملغي (9)
                var currentStatus = await _db.Set<PurchaseTxn>()
                    .Where(x => x.TxnID == txnId)
                    .Select(x => x.Status)
                    .FirstOrDefaultAsync();

                if (currentStatus == 9)
                {
                    System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.CancelAsync: Transaction {txnId} is already cancelled");
                    return false;
                }

                return await ChangeStatusAsync(txnId, 9);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PurchaseTxnsService.CancelAsync: Error cancelling transaction: {ex.Message}");
                return false;
            }
        }
    }
}
