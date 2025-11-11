using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Models;
using System.Windows;

namespace Next_Future_ERP.Features.StoreIssues.Services
{
    public class StoreIssuesService : IStoreIssuesService
    {
        private readonly AppDbContext _db;

        public StoreIssuesService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<StoreIssue>> GetAllAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("StoreIssuesService.GetAllAsync: Starting");
                var result = await _db.StoreIssues
                    .Include(x => x.Destination)
                    .Include(x => x.Currency)
                    .Include(x => x.DefaultWarehouse)
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetAllAsync: Found {result.Count} issues");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetAllAsync: Error: {ex.Message}");
                MessageBox.Show($"خطأ في جلب مستندات الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<StoreIssue>();
            }
        }

        public async Task<IEnumerable<StoreIssue>> GetAllAsync(string? searchText)
        {
            try
            {
                var query = _db.StoreIssues
                    .Include(x => x.Destination)
                    .Include(x => x.Currency)
                    .Include(x => x.DefaultWarehouse)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.IssueNumber.Contains(searchText) ||
                        x.Description.Contains(searchText) ||
                        x.ReferenceNumber.Contains(searchText) ||
                        (x.Destination != null && x.Destination.DestinationName.Contains(searchText)));
                }

                return await query
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetAllAsync(searchText): Error: {ex.Message}");
                MessageBox.Show($"خطأ في البحث عن مستندات الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<StoreIssue>();
            }
        }

        public async Task<StoreIssue?> GetByIdAsync(long id)
        {
            try
            {
                return await _db.StoreIssues
                    .Include(x => x.Destination)
                    .Include(x => x.Currency)
                    .Include(x => x.DefaultWarehouse)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IssueId == id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetByIdAsync: Error: {ex.Message}");
                MessageBox.Show($"خطأ في جلب مستند الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<StoreIssue?> GetByIdWithDetailsAsync(long id)
        {
            try
            {
                return await _db.StoreIssues
                    .Include(x => x.Destination)
                    .Include(x => x.Currency)
                    .Include(x => x.DefaultWarehouse)
                    .Include(x => x.Details)
                        .ThenInclude(d => d.Item)
                    .Include(x => x.Details)
                        .ThenInclude(d => d.Warehouse)
                    .Include(x => x.Details)
                        .ThenInclude(d => d.Unit)
                    .Include(x => x.Details)
                        .ThenInclude(d => d.CostCenter)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IssueId == id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetByIdWithDetailsAsync: Error: {ex.Message}");
                MessageBox.Show($"خطأ في جلب تفاصيل مستند الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<long> AddAsync(StoreIssue issue)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("StoreIssuesService.AddAsync: Starting add operation");
                
                // Generate issue number if not provided
                if (string.IsNullOrEmpty(issue.IssueNumber))
                {
                    issue.IssueNumber = await GenerateIssueNumberAsync(issue.CompanyId, issue.BranchId);
                }

                // Validate
                await ValidateAsync(issue);

                // Calculate totals
                await RecalculateTotalsAsync(issue);

                // Use transaction for atomic operations
                var strategy = _db.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        _db.StoreIssues.Add(issue);
                        await _db.SaveChangesAsync();

                        // Auto-fill accounts for details if they exist
                        if (issue.Details?.Any() == true)
                        {
                            foreach (var detail in issue.Details)
                            {
                                detail.IssueId = issue.IssueId;
                                await AutoFillDetailAccountsAsync(detail);
                            }
                            
                            _db.StoreIssuesDetailed.AddRange(issue.Details);
                            await _db.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                        System.Diagnostics.Debug.WriteLine($"StoreIssuesService.AddAsync: Successfully added issue with ID: {issue.IssueId}");
                        return issue.IssueId;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"StoreIssuesService.AddAsync: Transaction failed: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.AddAsync: Error: {ex.Message}");
                throw; // Re-throw to let ViewModel handle the error display
            }
        }

        public async Task UpdateAsync(StoreIssue issue)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.UpdateAsync: Starting update operation for ID: {issue.IssueId}");

                if (issue.IssueId <= 0)
                {
                    throw new InvalidOperationException("معرف المستند غير صالح");
                }

                // Check if issue is in draft status (can be modified)
                var existing = await _db.StoreIssues.FindAsync(issue.IssueId);
                if (existing?.Status != 0) // Not Draft
                {
                    throw new InvalidOperationException("لا يمكن تعديل مستند غير مسودة");
                }

                // Validate
                await ValidateAsync(issue);

                // Calculate totals
                await RecalculateTotalsAsync(issue);

                if (existing != null)
                {
                    _db.Entry(existing).CurrentValues.SetValues(issue);
                    existing.ModifiedAt = DateTime.Now;
                    await _db.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"StoreIssuesService.UpdateAsync: Successfully updated issue with ID: {issue.IssueId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.UpdateAsync: Error: {ex.Message}");
                throw; // Re-throw to let ViewModel handle the error display
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.DeleteAsync: Starting delete operation for ID: {id}");

                if (id <= 0)
                {
                    throw new InvalidOperationException("معرف المستند غير صالح");
                }

                var issue = await _db.StoreIssues
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.IssueId == id);

                if (issue != null)
                {
                    // Check if can be deleted (only Draft status)
                    if (issue.Status != 0) // Not Draft
                    {
                        throw new InvalidOperationException("لا يمكن حذف مستند غير مسودة");
                    }

                    // Use transaction for atomic operations
                    var strategy = _db.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _db.Database.BeginTransactionAsync();
                        try
                        {
                            // Delete details first (cascade should handle this, but explicit for safety)
                            _db.StoreIssuesDetailed.RemoveRange(issue.Details);
                            
                            // Delete the issue
                            _db.StoreIssues.Remove(issue);
                            
                            await _db.SaveChangesAsync();
                            await transaction.CommitAsync();
                            
                            System.Diagnostics.Debug.WriteLine($"StoreIssuesService.DeleteAsync: Successfully deleted issue with ID: {id}");
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            System.Diagnostics.Debug.WriteLine($"StoreIssuesService.DeleteAsync: Transaction failed: {ex.Message}");
                            throw;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.DeleteAsync: Error: {ex.Message}");
                MessageBox.Show($"خطأ في حذف مستند الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> PostAsync(long id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.PostAsync: Starting post operation for ID: {id}");

                var issue = await _db.StoreIssues
                    .Include(x => x.Details)
                    .Include(x => x.Destination)
                    .FirstOrDefaultAsync(x => x.IssueId == id);

                if (issue == null)
                {
                    throw new InvalidOperationException("مستند الصرف غير موجود");
                }

                if (issue.Status != 0) // Not Draft
                {
                    throw new InvalidOperationException("المستند غير مسودة");
                }

                // Validate before posting
                await ValidateForPostingAsync(issue);

                // Use transaction for atomic operations
                var strategy = _db.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        // 1. Update inventory balances
                        await UpdateInventoryBalancesAsync(issue);

                        // 2. Create accounting entries
                        await CreateAccountingEntriesAsync(issue);

                        // 3. Update issue status
                        issue.Status = 1; // Posted
                        issue.ModifiedAt = DateTime.Now;
                        await _db.SaveChangesAsync();

                        await transaction.CommitAsync();
                        System.Diagnostics.Debug.WriteLine($"StoreIssuesService.PostAsync: Successfully posted issue with ID: {id}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"StoreIssuesService.PostAsync: Transaction failed: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.PostAsync: Error: {ex.Message}");
                MessageBox.Show($"خطأ في ترحيل مستند الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> CancelAsync(long id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.CancelAsync: Starting cancel operation for ID: {id}");

                var issue = await _db.StoreIssues
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.IssueId == id);

                if (issue == null)
                {
                    throw new InvalidOperationException("مستند الصرف غير موجود");
                }

                if (issue.Status == 2) // Already Canceled
                {
                    throw new InvalidOperationException("المستند ملغي بالفعل");
                }

                // If posted, reverse the effects first
                if (issue.Status == 1) // Posted
                {
                    await ReversePostingEffectsAsync(issue);
                }

                issue.Status = 2; // Canceled
                issue.ModifiedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.CancelAsync: Successfully canceled issue with ID: {id}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.CancelAsync: Error: {ex.Message}");
                MessageBox.Show($"خطأ في إلغاء مستند الصرف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<string> GenerateIssueNumberAsync(int companyId, int branchId)
        {
            try
            {
                var lastNumber = await _db.StoreIssues
                    .Where(x => x.CompanyId == companyId && x.BranchId == branchId)
                    .OrderByDescending(x => x.IssueId)
                    .Select(x => x.IssueNumber)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(lastNumber))
                {
                    return $"SI{companyId:D2}{branchId:D2}0001";
                }

                // Extract number and increment
                var numberPart = lastNumber.Substring(6); // After "SI" + company + branch
                if (int.TryParse(numberPart, out int number))
                {
                    return $"SI{companyId:D2}{branchId:D2}{(number + 1):D4}";
                }

                return $"SI{companyId:D2}{branchId:D2}0001";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GenerateIssueNumberAsync: Error: {ex.Message}");
                return $"SI{companyId:D2}{branchId:D2}{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        public async Task<bool> IsIssueNumberUniqueAsync(string issueNumber, int companyId, int branchId, long? excludeId = null)
        {
            try
            {
                var query = _db.StoreIssues
                    .Where(x => x.CompanyId == companyId && x.BranchId == branchId && x.IssueNumber == issueNumber);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.IssueId != excludeId.Value);
                }

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.IsIssueNumberUniqueAsync: Error: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<StoreIssueDetail>> GetDetailsAsync(long issueId)
        {
            try
            {
                return await _db.StoreIssuesDetailed
                    .Include(x => x.Item)
                    .Include(x => x.Warehouse)
                    .Include(x => x.Unit)
                    .Include(x => x.CostCenter)
                    .Where(x => x.IssueId == issueId)
                    .OrderBy(x => x.LineNo)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetDetailsAsync: Error: {ex.Message}");
                return new List<StoreIssueDetail>();
            }
        }

        public async Task AddDetailAsync(StoreIssueDetail detail)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.AddDetailAsync: Starting add detail for IssueId: {detail.IssueId}");

                // Validate required fields
                if (detail.IssueId <= 0)
                {
                    throw new InvalidOperationException("معرف المستند مطلوب");
                }

                if (detail.WarehouseId <= 0)
                {
                    throw new InvalidOperationException("معرف المخزن مطلوب");
                }

                // Auto-assign line number
                var maxLineNo = await _db.StoreIssuesDetailed
                    .Where(x => x.IssueId == detail.IssueId)
                    .MaxAsync(x => (int?)x.LineNo) ?? 0;

                detail.LineNo = maxLineNo + 1;

                // Auto-fill accounts and cost center
                await AutoFillDetailAccountsAsync(detail);

                _db.StoreIssuesDetailed.Add(detail);
                await _db.SaveChangesAsync();

                // Recalculate totals
                await RecalculateTotalsAsync(detail.IssueId);

                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.AddDetailAsync: Successfully added detail with ID: {detail.DetailId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.AddDetailAsync: Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateDetailAsync(StoreIssueDetail detail)
        {
            try
            {
                var existing = await _db.StoreIssuesDetailed.FindAsync(detail.DetailId);
                if (existing != null)
                {
                    // Auto-fill accounts if not already set
                    await AutoFillDetailAccountsAsync(detail);
                    
                    _db.Entry(existing).CurrentValues.SetValues(detail);
                    await _db.SaveChangesAsync();

                    // Recalculate totals
                    await RecalculateTotalsAsync(detail.IssueId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.UpdateDetailAsync: Error: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDetailAsync(long detailId)
        {
            try
            {
                var detail = await _db.StoreIssuesDetailed.FindAsync(detailId);
                if (detail != null)
                {
                    var issueId = detail.IssueId;
                    _db.StoreIssuesDetailed.Remove(detail);
                    await _db.SaveChangesAsync();

                    // Recalculate totals
                    await RecalculateTotalsAsync(issueId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.DeleteDetailAsync: Error: {ex.Message}");
                throw;
            }
        }

        public async Task RecalculateTotalsAsync(long issueId)
        {
            try
            {
                var issue = await _db.StoreIssues.FindAsync(issueId);
                if (issue != null)
                {
                    var totalAmount = await _db.StoreIssuesDetailed
                        .Where(x => x.IssueId == issueId)
                        .SumAsync(x => x.TotalCost);

                    issue.TotalAmount = totalAmount;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.RecalculateTotalsAsync: Error: {ex.Message}");
                throw;
            }
        }

        private async Task RecalculateTotalsAsync(StoreIssue issue)
        {
            if (issue.Details != null && issue.Details.Any())
            {
                issue.TotalAmount = issue.Details.Sum(x => x.TotalCost);
            }
            else
            {
                issue.TotalAmount = 0;
            }
        }

        private async Task ValidateAsync(StoreIssue issue)
        {
            var errors = new List<string>();

            // التحققات الأساسية مع رسائل واضحة
            if (issue.CompanyId <= 0) 
                errors.Add("• معرف الشركة مطلوب ولا يمكن أن يكون فارغاً");
            
            if (issue.BranchId <= 0) 
                errors.Add("• معرف الفرع مطلوب ولا يمكن أن يكون فارغاً");
            
            if (string.IsNullOrWhiteSpace(issue.IssueNumber)) 
                errors.Add("• رقم المستند مطلوب ولا يمكن أن يكون فارغاً");
            
            if (issue.IssueDate == default) 
                errors.Add("• تاريخ المستند مطلوب - يرجى تحديد تاريخ صحيح");
            
            if (issue.IssueDestinationID <= 0) 
                errors.Add("• جهة الصرف مطلوبة - يرجى اختيار جهة صرف صالحة");
            
            if (issue.CurrencyId <= 0) 
                errors.Add("• العملة مطلوبة - يرجى اختيار عملة صالحة");
            
            if (issue.ExchangeRate <= 0) 
                errors.Add("• سعر الصرف مطلوب ويجب أن يكون أكبر من صفر");

            // التحقق من عدم تكرار رقم المستند
            var isUnique = await IsIssueNumberUniqueAsync(issue.IssueNumber, issue.CompanyId, issue.BranchId, issue.IssueId);
            if (!isUnique)
            {
                errors.Add($"• رقم المستند '{issue.IssueNumber}' مكرر - يرجى استخدام رقم مختلف");
            }

            // التحقق من وجود جهة الصرف ونشاطها
            var destination = await _db.IssueDestinations.FindAsync(issue.IssueDestinationID);
            if (destination == null)
            {
                errors.Add($"• جهة الصرف المحددة (ID: {issue.IssueDestinationID}) غير موجودة في النظام");
            }
            else if (!destination.IsActive)
            {
                errors.Add($"• جهة الصرف '{destination.DestinationName}' غير نشطة - يرجى اختيار جهة نشطة");
            }

            // التحقق من وجود العملة
            var currency = await _db.NextCurrencies.FindAsync(issue.CurrencyId);
            if (currency == null)
            {
                errors.Add($"• العملة المحددة (ID: {issue.CurrencyId}) غير موجودة في النظام");
            }

            // التحقق من المخزن الافتراضي إذا كان محدداً
            if (issue.DefaultWarehouseId.HasValue)
            {
                var warehouse = await _db.Warehouses.FindAsync(issue.DefaultWarehouseId.Value);
                if (warehouse == null)
                {
                    errors.Add($"• المخزن الافتراضي المحدد (ID: {issue.DefaultWarehouseId}) غير موجود في النظام");
                }
                else if (warehouse.IsActive != true)
                {
                    errors.Add($"• المخزن الافتراضي '{warehouse.WarehouseName}' غير نشط - يرجى اختيار مخزن نشط");
                }
            }

            // التحقق من فترة مالية مفتوحة (إذا كان الجدول موجود)
            await ValidateFinancialPeriodAsync(issue.IssueDate, issue.CompanyId, issue.BranchId, errors);

            // التحقق من وجود السطور
            if (issue.Details == null || !issue.Details.Any())
            {
                errors.Add("• يجب إضافة سطر واحد على الأقل في التفاصيل");
            }
            else
            {
                // التحقق من كل سطر مع رسائل مفصلة
                foreach (var detail in issue.Details)
                {
                    var lineErrors = await ValidateDetailAsync(detail);
                    errors.AddRange(lineErrors);
                }
            }

            // إذا وُجدت أخطاء، ارمي استثناء مع جميع الأخطاء
            if (errors.Any())
            {
                var errorMessage = "تم العثور على الأخطاء التالية:\n\n" + string.Join("\n", errors);
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// التحقق من فترة مالية مفتوحة
        /// </summary>
        private async Task ValidateFinancialPeriodAsync(DateTime issueDate, int companyId, int branchId, List<string> errors)
        {
            try
            {
                // التحقق من وجود جدول FinancialPeriods (إذا كان موجوداً)
                var hasFinancialPeriods = await _db.Database.ExecuteSqlRawAsync("SELECT 1 FROM sys.tables WHERE name = 'FinancialPeriods'") > 0;
                
                if (hasFinancialPeriods)
                {
                    // البحث عن فترة مالية مفتوحة للتاريخ المحدد
                    var openPeriod = await _db.Database.SqlQueryRaw<int>(
                        "SELECT COUNT(*) FROM FinancialPeriods WHERE CompanyId = {0} AND BranchId = {1} AND StartDate <= {2} AND EndDate >= {2} AND IsOpen = 1 AND IsLocked = 0",
                        companyId, branchId, issueDate).FirstOrDefaultAsync();

                    if (openPeriod == 0)
                    {
                        errors.Add($"• تاريخ المستند ({issueDate:yyyy-MM-dd}) خارج الفترة المالية المفتوحة - يرجى اختيار تاريخ ضمن فترة مالية مفتوحة");
                    }
                }
            }
            catch (Exception ex)
            {
                // إذا فشل التحقق من الفترة المالية، سجل تحذير فقط
                System.Diagnostics.Debug.WriteLine($"ValidateFinancialPeriodAsync: Warning - Could not validate financial period: {ex.Message}");
            }
        }

        /// <summary>
        /// التحقق من صحة سطر التفصيل مع رسائل مفصلة
        /// </summary>
        private async Task<List<string>> ValidateDetailAsync(StoreIssueDetail detail)
        {
            var errors = new List<string>();
            var linePrefix = $"السطر {detail.LineNo}";

            // التحققات الأساسية للسطر
            if (detail.ItemId <= 0)
                errors.Add($"• {linePrefix}: الصنف مطلوب - يرجى اختيار صنف صالح");

            if (detail.WarehouseId <= 0)
                errors.Add($"• {linePrefix}: المخزن مطلوب - يرجى اختيار مخزن صالح");

            if (detail.Quantity <= 0)
                errors.Add($"• {linePrefix}: الكمية يجب أن تكون أكبر من صفر (القيمة الحالية: {detail.Quantity:N3})");

            if (detail.UnitCost < 0)
                errors.Add($"• {linePrefix}: تكلفة الوحدة يجب أن تكون أكبر من أو تساوي صفر (القيمة الحالية: {detail.UnitCost:N2})");

            // التحقق من وجود الصنف
            if (detail.ItemId > 0)
            {
                var item = await _db.Items.FindAsync(detail.ItemId);
                if (item == null)
                {
                    errors.Add($"• {linePrefix}: الصنف المحدد (ID: {detail.ItemId}) غير موجود في النظام");
                }
                else if (item.IsActive != true)
                {
                    errors.Add($"• {linePrefix}: الصنف '{item.ItemName}' غير نشط - يرجى اختيار صنف نشط");
                }
            }

            // التحقق من وجود المخزن
            if (detail.WarehouseId > 0)
            {
                var warehouse = await _db.Warehouses.FindAsync(detail.WarehouseId);
                if (warehouse == null)
                {
                    errors.Add($"• {linePrefix}: المخزن المحدد (ID: {detail.WarehouseId}) غير موجود في النظام");
                }
                else if (warehouse.IsActive != true)
                {
                    errors.Add($"• {linePrefix}: المخزن '{warehouse.WarehouseName}' غير نشط - يرجى اختيار مخزن نشط");
                }
            }

            // التحقق من تتبع الدفعات
            if (detail.ItemId > 0)
            {
                var item = await _db.Items.FindAsync(detail.ItemId);
                if (item?.IsBatchTracked == true)
                {
                    if (detail.BatchID == null || string.IsNullOrEmpty(detail.BatchNumber))
                    {
                        errors.Add($"• {linePrefix}: الصنف '{item.ItemName}' يتطلب تحديد رقم الدفعة");
                    }
                }
            }

            // التحقق من الصلاحية
            if (detail.ItemId > 0)
            {
                var item = await _db.Items.FindAsync(detail.ItemId);
                if (item?.HasExpiryDate == true && detail.ExpiryDate.HasValue)
                {
                    if (detail.ExpiryDate.Value < DateTime.Today)
                    {
                        errors.Add($"• {linePrefix}: الدفعة منتهية الصلاحية بتاريخ {detail.ExpiryDate.Value:yyyy-MM-dd}");
                    }
                }
            }

            // التحقق من حساب المخزون
            if (detail.ItemId > 0 && detail.WarehouseId > 0)
            {
                var inventoryAccount = await GetInventoryAccountAsync(detail.WarehouseId, detail.ItemId);
                if (string.IsNullOrEmpty(inventoryAccount))
                {
                    errors.Add($"• {linePrefix}: حساب المخزون غير محدد للمخزن والصنف - يرجى إعداد ValuationGroupAccounts");
                }
            }

            // التحقق من الوحدة إذا كانت محددة
            if (detail.UnitId.HasValue && detail.UnitId > 0)
            {
                var unit = await _db.Units.FindAsync(detail.UnitId.Value);
                if (unit == null)
                {
                    errors.Add($"• {linePrefix}: الوحدة المحددة (ID: {detail.UnitId}) غير موجودة في النظام");
                }
                else if (unit.IsActive != true)
                {
                    errors.Add($"• {linePrefix}: الوحدة '{unit.UnitName}' غير نشطة - يرجى اختيار وحدة نشطة");
                }
            }

            // التحقق من مركز الكلفة إذا كان محدداً
            if (detail.CostCenterId.HasValue && detail.CostCenterId > 0)
            {
                var costCenter = await _db.CostCenter.FindAsync(detail.CostCenterId.Value);
                if (costCenter == null)
                {
                    errors.Add($"• {linePrefix}: مركز الكلفة المحدد (ID: {detail.CostCenterId}) غير موجود في النظام");
                }
                else if (costCenter.IsActive != true)
                {
                    errors.Add($"• {linePrefix}: مركز الكلفة '{costCenter.CostCenterName}' غير نشط - يرجى اختيار مركز كلفة نشط");
                }
            }

            // التحقق من الدفعة إذا كانت محددة
            if (detail.BatchID.HasValue && detail.BatchID > 0)
            {
                var batch = await _db.ItemBatches.FindAsync(detail.BatchID.Value);
                if (batch == null)
                {
                    errors.Add($"• {linePrefix}: الدفعة المحددة (ID: {detail.BatchID}) غير موجودة في النظام");
                }
                else if (batch.IsActive != true)
                {
                    errors.Add($"• {linePrefix}: الدفعة '{batch.BatchNumber}' غير نشطة - يرجى اختيار دفعة نشطة");
                }
            }

            // التحقق من منطقية التكلفة الإجمالية
            var calculatedTotalCost = detail.Quantity * detail.UnitCost;
            if (Math.Abs(detail.TotalCost - calculatedTotalCost) > 0.01m)
            {
                errors.Add($"• {linePrefix}: التكلفة الإجمالية غير متطابقة - المحسوبة: {calculatedTotalCost:N2}, المدخلة: {detail.TotalCost:N2}");
            }

            // التحقق من حدود الكمية المعقولة
            if (detail.Quantity > 1000000)
            {
                errors.Add($"• {linePrefix}: الكمية كبيرة جداً ({detail.Quantity:N3}) - يرجى التحقق من صحة القيمة");
            }

            // التحقق من حدود التكلفة المعقولة
            if (detail.UnitCost > 1000000)
            {
                errors.Add($"• {linePrefix}: تكلفة الوحدة كبيرة جداً ({detail.UnitCost:N2}) - يرجى التحقق من صحة القيمة");
            }

            return errors;
        }

        private async Task ValidateForPostingAsync(StoreIssue issue)
        {
            var errors = new List<string>();

            // التحقق من نشاط جهة الصرف
            var destination = await _db.IssueDestinations.FindAsync(issue.IssueDestinationID);
            if (destination == null || !destination.IsActive)
            {
                errors.Add("• جهة الصرف غير نشطة - لا يمكن ترحيل المستند");
            }

            // التحقق من وجود السطور
            if (!issue.Details.Any())
            {
                errors.Add("• يجب إضافة سطر واحد على الأقل قبل الترحيل");
            }
            else
            {
                // التحقق من كل سطر قبل الترحيل
                foreach (var detail in issue.Details)
                {
                    var lineErrors = await ValidateDetailForPostingAsync(detail);
                    errors.AddRange(lineErrors);
                }
            }

            // إذا وُجدت أخطاء، ارمي استثناء مع جميع الأخطاء
            if (errors.Any())
            {
                var errorMessage = "لا يمكن ترحيل المستند للأسباب التالية:\n\n" + string.Join("\n", errors);
                throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// التحقق من صحة سطر التفصيل قبل الترحيل
        /// </summary>
        private async Task<List<string>> ValidateDetailForPostingAsync(StoreIssueDetail detail)
        {
            var errors = new List<string>();
            var linePrefix = $"السطر {detail.LineNo}";

            // التحقق من الكمية المتاحة
            try
            {
                await ValidateAvailableQuantityAsync(detail);
            }
            catch (InvalidOperationException ex)
            {
                errors.Add($"• {linePrefix}: {ex.Message}");
            }

            // التحقق من مركز الكلفة إذا كانت الجهة تتطلبه
            var issue = await _db.StoreIssues
                .Include(x => x.Destination)
                .FirstOrDefaultAsync(x => x.IssueId == detail.IssueId);

            if (issue?.Destination?.UsesCostCenter == true && !detail.CostCenterId.HasValue)
            {
                errors.Add($"• {linePrefix}: مركز الكلفة مطلوب لجهة الصرف '{issue.Destination.DestinationName}'");
            }

            // التحقق من الحسابات المحاسبية
            if (string.IsNullOrEmpty(detail.DebitAccount))
            {
                errors.Add($"• {linePrefix}: الحساب المدين غير محدد");
            }

            if (string.IsNullOrEmpty(detail.CreditAccount))
            {
                errors.Add($"• {linePrefix}: الحساب الدائن (حساب المخزون) غير محدد");
            }

            return errors;
        }

        private async Task UpdateInventoryBalancesAsync(StoreIssue issue)
        {
            foreach (var detail in issue.Details)
            {
                var balance = await _db.InventoryBalances
                    .FirstOrDefaultAsync(x => x.WarehouseID == detail.WarehouseId 
                                           && x.ItemID == detail.ItemId 
                                           && x.BatchID == detail.BatchID);

                if (balance != null)
                {
                    balance.Quantity -= detail.Quantity;
                    balance.LastCost = detail.UnitCost;
                    balance.LastUpdate = DateTime.Now;
                    
                    if (balance.Quantity < 0)
                    {
                        throw new InvalidOperationException($"السطر {detail.LineNo}: الكمية المتاحة غير كافية");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"السطر {detail.LineNo}: رصيد المخزون غير موجود");
                }
            }
        }

        private async Task CreateAccountingEntriesAsync(StoreIssue issue)
        {
            var journalEntry = new GeneralJournalEntry
            {
                CompanyId = issue.CompanyId,
                BranchId = issue.BranchId,
                DocumentTypeId = 1, // Store Issue Document Type
                DocumentNumber = issue.IssueNumber,
                PostingDate = issue.IssueDate,
                ReferenceNumber = issue.ReferenceNumber,
                Description = $"صرف مخزني - {issue.IssueNumber}",
                Status = 1, // Posted
                CreatedBy = issue.CreatedBy,
                CreatedAt = DateTime.Now
            };

            var details = new List<GeneralJournalEntryDetail>();

            foreach (var detail in issue.Details)
            {
                // Get inventory account
                var inventoryAccount = await GetInventoryAccountAsync(detail.WarehouseId, detail.ItemId);
                
                // Debit entry (from destination)
                details.Add(new GeneralJournalEntryDetail
                {
                    AccountNumber = detail.DebitAccount ?? issue.Destination?.Account?.AccountCode ?? "",
                    CostCenterId = detail.CostCenterId,
                    Statement = $"صرف مخزني - {detail.ItemName}",
                    DebitAmount = detail.TotalCost,
                    CurrencyId = detail.CurrencyId,
                    ExchangeRate = detail.ExchangeRate,
                    CreatedAt = DateTime.Now
                });

                // Credit entry (inventory account)
                details.Add(new GeneralJournalEntryDetail
                {
                    AccountNumber = inventoryAccount,
                    CostCenterId = detail.CostCenterId,
                    Statement = $"صرف مخزني - {detail.ItemName}",
                    CreditAmount = detail.TotalCost,
                    CurrencyId = detail.CurrencyId,
                    ExchangeRate = detail.ExchangeRate,
                    CreatedAt = DateTime.Now
                });
            }

            journalEntry.Details = details;
            journalEntry.TotalDebit = details.Sum(d => d.DebitAmount ?? 0);
            journalEntry.TotalCredit = details.Sum(d => d.CreditAmount ?? 0);

            _db.GeneralJournalEntries.Add(journalEntry);
        }

        private async Task<string> GetInventoryAccountAsync(int warehouseId, int itemId)
        {
            // Get item category
            var item = await _db.Items
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.ItemID == itemId);

            if (item?.Category == null)
                return string.Empty;

            // Get valuation group account for the warehouse and category
            var valuationAccount = await _db.ValuationGroupAccounts
                .FirstOrDefaultAsync(x => x.CompanyId == 1 && x.ValuationGroup == item.Category.CategoryID);

            return valuationAccount?.InventoryAcc ?? string.Empty;
        }

        private async Task ValidateAvailableQuantityAsync(StoreIssueDetail detail)
        {
            // التحقق من سياسة السماح بالسالب في المخزن
            var warehouse = await _db.Warehouses.FindAsync(detail.WarehouseId);
            if (warehouse == null)
            {
                throw new InvalidOperationException($"السطر {detail.LineNo}: المخزن غير موجود");
            }

            var balance = await _db.InventoryBalances
                .FirstOrDefaultAsync(x => x.WarehouseID == detail.WarehouseId 
                                       && x.ItemID == detail.ItemId 
                                       && x.BatchID == detail.BatchID);

            var availableQuantity = balance?.Quantity ?? 0;

            // إذا كان المخزن لا يسمح بالسالب والكمية المتاحة غير كافية
            if (warehouse.AllowNegativeStock != true && availableQuantity < detail.Quantity)
            {
                throw new InvalidOperationException($"السطر {detail.LineNo}: الكمية المتاحة ({availableQuantity:N2}) أقل من المطلوب ({detail.Quantity:N2}). المخزن '{warehouse.WarehouseName}' لا يسمح بالمخزون السالب.");
            }

            // تحذير إذا كان المخزون سالب حتى لو كان مسموحاً
            if (warehouse.AllowNegativeStock == true && availableQuantity < detail.Quantity)
            {
                System.Diagnostics.Debug.WriteLine($"تحذير: السطر {detail.LineNo} سيؤدي إلى مخزون سالب ({availableQuantity - detail.Quantity:N2}) في المخزن '{warehouse.WarehouseName}'");
            }
        }

        private async Task AutoFillDetailAccountsAsync(StoreIssueDetail detail)
        {
            try
            {
                // Get the issue header
                var issue = await _db.StoreIssues
                    .Include(x => x.Destination)
                    .FirstOrDefaultAsync(x => x.IssueId == detail.IssueId);

                if (issue?.Destination == null)
                {
                    System.Diagnostics.Debug.WriteLine($"AutoFillDetailAccountsAsync: Issue or Destination not found for IssueId: {detail.IssueId}");
                    return;
                }

                // Auto-fill debit account from destination
                if (string.IsNullOrEmpty(detail.DebitAccount))
                {
                    detail.DebitAccount = issue.Destination.AccountID?.ToString() ?? "";
                }

                // Auto-fill cost center from destination
                await AutoFillCostCenterAsync(detail, issue.Destination);

                // Auto-fill UnitCost from inventory balance
                await AutoFillUnitCostAsync(detail);

                // Auto-fill credit account (inventory account)
                await AutoFillCreditAccountAsync(detail);

                // Inherit currency and exchange rate from header
                detail.CurrencyId = issue.CurrencyId;
                detail.ExchangeRate = issue.ExchangeRate;

                System.Diagnostics.Debug.WriteLine($"AutoFillDetailAccountsAsync: Successfully filled accounts for detail");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoFillDetailAccountsAsync: Error: {ex.Message}");
                // Set default values to avoid blocking the operation
                detail.DebitAccount = detail.DebitAccount ?? "0000";
                detail.CreditAccount = detail.CreditAccount ?? "0000";
                detail.UnitCost = detail.UnitCost == 0 ? 1 : detail.UnitCost;
            }
        }

        private async Task ValidateBatchTrackingAsync(StoreIssueDetail detail)
        {
            // Get item properties
            var item = await _db.Items.FirstOrDefaultAsync(x => x.ItemID == detail.ItemId);
            if (item == null) return;

            // If item requires batch tracking, validate batch is provided
            if (item.IsBatchTracked == true)
            {
                if (detail.BatchID == null && string.IsNullOrEmpty(detail.BatchNumber))
                {
                    throw new InvalidOperationException($"السطر {detail.LineNo}: الصنف يتطلب تتبع الدفعة");
                }
            }
        }

        private async Task ValidateExpiryDateAsync(StoreIssueDetail detail)
        {
            // Get item properties
            var item = await _db.Items.FirstOrDefaultAsync(x => x.ItemID == detail.ItemId);
            if (item == null) return;

            // If item has expiry date, validate it
            if (item.HasExpiryDate == true)
            {
                if (detail.ExpiryDate == null)
                {
                    throw new InvalidOperationException($"السطر {detail.LineNo}: الصنف يتطلب تاريخ انتهاء الصلاحية");
                }

                // Check if expired (optional policy check)
                if (detail.ExpiryDate < DateTime.Today)
                {
                    // This could be a warning instead of error based on policy
                    System.Diagnostics.Debug.WriteLine($"Warning: Item {detail.ItemId} is expired on line {detail.LineNo}");
                }
            }
        }

        private async Task ReversePostingEffectsAsync(StoreIssue issue)
        {
            // Use transaction for atomic operations
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    // 1. Reverse inventory balances
                    await ReverseInventoryBalancesAsync(issue);

                    // 2. Create reversal accounting entries
                    await CreateReversalAccountingEntriesAsync(issue);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"StoreIssuesService.ReversePostingEffectsAsync: Transaction failed: {ex.Message}");
                    throw;
                }
            });
        }

        private async Task ReverseInventoryBalancesAsync(StoreIssue issue)
        {
            foreach (var detail in issue.Details)
            {
                var balance = await _db.InventoryBalances
                    .FirstOrDefaultAsync(x => x.WarehouseID == detail.WarehouseId 
                                           && x.ItemID == detail.ItemId 
                                           && x.BatchID == detail.BatchID);

                if (balance != null)
                {
                    balance.Quantity += detail.Quantity; // Add back the quantity
                    balance.LastUpdate = DateTime.Now;
                }
            }
        }

        private async Task CreateReversalAccountingEntriesAsync(StoreIssue issue)
        {
            var journalEntry = new GeneralJournalEntry
            {
                CompanyId = issue.CompanyId,
                BranchId = issue.BranchId,
                DocumentTypeId = 1, // Store Issue Document Type
                DocumentNumber = $"{issue.IssueNumber}-REV",
                PostingDate = DateTime.Today,
                ReferenceNumber = issue.ReferenceNumber,
                Description = $"عكس صرف مخزني - {issue.IssueNumber}",
                Status = 1, // Posted
                CreatedBy = issue.CreatedBy,
                CreatedAt = DateTime.Now
            };

            var details = new List<GeneralJournalEntryDetail>();

            foreach (var detail in issue.Details)
            {
                // Get inventory account
                var inventoryAccount = await GetInventoryAccountAsync(detail.WarehouseId, detail.ItemId);
                
                // Reverse debit entry (credit the destination account)
                details.Add(new GeneralJournalEntryDetail
                {
                    AccountNumber = detail.DebitAccount ?? issue.Destination?.Account?.AccountCode ?? "",
                    CostCenterId = detail.CostCenterId,
                    Statement = $"عكس صرف مخزني - {detail.ItemName}",
                    CreditAmount = detail.TotalCost, // Credit instead of debit
                    CurrencyId = detail.CurrencyId,
                    ExchangeRate = detail.ExchangeRate,
                    CreatedAt = DateTime.Now
                });

                // Reverse credit entry (debit the inventory account)
                details.Add(new GeneralJournalEntryDetail
                {
                    AccountNumber = inventoryAccount,
                    CostCenterId = detail.CostCenterId,
                    Statement = $"عكس صرف مخزني - {detail.ItemName}",
                    DebitAmount = detail.TotalCost, // Debit instead of credit
                    CurrencyId = detail.CurrencyId,
                    ExchangeRate = detail.ExchangeRate,
                    CreatedAt = DateTime.Now
                });
            }

            journalEntry.Details = details;
            journalEntry.TotalDebit = details.Sum(d => d.DebitAmount ?? 0);
            journalEntry.TotalCredit = details.Sum(d => d.CreditAmount ?? 0);

            _db.GeneralJournalEntries.Add(journalEntry);
        }

        public async Task<Dictionary<string, object>> PreviewAccountingEntriesAsync(long issueId)
        {
            try
            {
                var issue = await _db.StoreIssues
                    .Include(x => x.Details)
                    .Include(x => x.Destination)
                    .FirstOrDefaultAsync(x => x.IssueId == issueId);

                if (issue == null)
                    throw new InvalidOperationException("مستند الصرف غير موجود");

                var previewData = new Dictionary<string, object>();
                var entries = new List<object>();
                var warnings = new List<string>();

                foreach (var detail in issue.Details)
                {
                    // Get inventory account
                    var inventoryAccount = await GetInventoryAccountAsync(detail.WarehouseId, detail.ItemId);
                    
                    if (string.IsNullOrEmpty(inventoryAccount))
                    {
                        warnings.Add($"السطر {detail.LineNo}: حساب المخزون غير محدد");
                    }

                    // Debit entry
                    entries.Add(new
                    {
                        AccountCode = detail.DebitAccount ?? issue.Destination?.Account?.AccountCode ?? "",
                        AccountName = issue.Destination?.Account?.AccountNameAr ?? "",
                        CostCenterId = detail.CostCenterId,
                        CostCenterName = detail.CostCenterName,
                        Statement = $"صرف مخزني - {detail.ItemName}",
                        DebitAmount = detail.TotalCost,
                        CreditAmount = (decimal?)null,
                        CurrencyId = detail.CurrencyId,
                        ExchangeRate = detail.ExchangeRate
                    });

                    // Credit entry
                    entries.Add(new
                    {
                        AccountCode = inventoryAccount,
                        AccountName = "حساب المخزون",
                        CostCenterId = detail.CostCenterId,
                        CostCenterName = detail.CostCenterName,
                        Statement = $"صرف مخزني - {detail.ItemName}",
                        DebitAmount = (decimal?)null,
                        CreditAmount = detail.TotalCost,
                        CurrencyId = detail.CurrencyId,
                        ExchangeRate = detail.ExchangeRate
                    });
                }

                previewData["Entries"] = entries;
                previewData["TotalDebit"] = entries.Sum(e => ((dynamic)e).DebitAmount ?? 0);
                previewData["TotalCredit"] = entries.Sum(e => ((dynamic)e).CreditAmount ?? 0);
                previewData["Warnings"] = warnings;
                previewData["IssueNumber"] = issue.IssueNumber;
                previewData["IssueDate"] = issue.IssueDate;
                previewData["DestinationName"] = issue.DestinationName;

                return previewData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.PreviewAccountingEntriesAsync: Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetInventoryInfoAsync(int warehouseId, int itemId, int? batchId = null)
        {
            try
            {
                var info = new Dictionary<string, object>();

                // Get inventory balance
                var balance = await _db.InventoryBalances
                    .Include(x => x.Item)
                    .Include(x => x.Warehouse)
                    .Include(x => x.Batch)
                    .Include(x => x.Unit)
                    .FirstOrDefaultAsync(x => x.WarehouseID == warehouseId 
                                           && x.ItemID == itemId 
                                           && x.BatchID == batchId);

                if (balance != null)
                {
                    info["AvailableQuantity"] = balance.Quantity;
                    info["AvgCost"] = balance.AvgCost;
                    info["LastCost"] = balance.LastCost;
                    info["UnitName"] = balance.UnitName;
                    info["BatchNumber"] = balance.Batch?.BatchNumber ?? "";
                    info["ExpiryDate"] = balance.Batch?.ExpiryDate;
                    info["IsExpired"] = balance.Batch?.ExpiryDate < DateTime.Today;
                }
                else
                {
                    info["AvailableQuantity"] = 0;
                    info["AvgCost"] = 0;
                    info["LastCost"] = 0;
                    info["UnitName"] = "";
                    info["BatchNumber"] = "";
                    info["ExpiryDate"] = null;
                    info["IsExpired"] = false;
                }

                // Get item properties
                var item = await _db.Items.FirstOrDefaultAsync(x => x.ItemID == itemId);
                if (item != null)
                {
                    info["ItemName"] = item.ItemName;
                    info["ItemCode"] = item.ItemCode;
                    info["IsBatchTracked"] = item.IsBatchTracked;
                    info["HasExpiryDate"] = item.HasExpiryDate;
                    info["StandardCost"] = item.StandardCost;
                }

                return info;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreIssuesService.GetInventoryInfoAsync: Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// تعبئة الحساب الدائن (حساب المخزون) تلقائياً
        /// </summary>
        private async Task AutoFillCreditAccountAsync(StoreIssueDetail detail)
        {
            try
            {
                // إذا كان الحساب الدائن محدد مسبقاً، لا نغيرها
                if (!string.IsNullOrEmpty(detail.CreditAccount))
                {
                    System.Diagnostics.Debug.WriteLine($"AutoFillCreditAccountAsync: CreditAccount already set to {detail.CreditAccount} for detail {detail.DetailId}");
                    return;
                }

                // البحث عن حساب المخزون من ValuationGroupAccounts
                var inventoryAccount = await GetInventoryAccountAsync(detail.WarehouseId, detail.ItemId);
                
                if (!string.IsNullOrEmpty(inventoryAccount))
                {
                    detail.CreditAccount = inventoryAccount;
                    System.Diagnostics.Debug.WriteLine($"AutoFillCreditAccountAsync: Set CreditAccount to {inventoryAccount} for detail {detail.DetailId}");
                }
                else
                {
                    // استخدام حساب افتراضي إذا لم يوجد حساب مخزون محدد
                    detail.CreditAccount = "0000"; // Default inventory account
                    System.Diagnostics.Debug.WriteLine($"AutoFillCreditAccountAsync: Warning - No inventory account found, using default '0000' for detail {detail.DetailId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoFillCreditAccountAsync: Error: {ex.Message}");
                // في حالة الخطأ، استخدم حساب افتراضي
                detail.CreditAccount = "0000";
            }
        }

        /// <summary>
        /// تعبئة مركز الكلفة تلقائياً من جهة الصرف
        /// </summary>
        private async Task AutoFillCostCenterAsync(StoreIssueDetail detail, IssueDestination destination)
        {
            try
            {
                // إذا كان مركز الكلفة محدد مسبقاً، لا نغيرها إلا إذا كانت الجهة تتطلب مركز كلفة
                if (detail.CostCenterId.HasValue && !destination.UsesCostCenter)
                {
                    System.Diagnostics.Debug.WriteLine($"AutoFillCostCenterAsync: CostCenterId already set to {detail.CostCenterId} for detail {detail.DetailId}");
                    return;
                }

                // إذا كانت الجهة تتطلب مركز كلفة ولم يكن محدداً
                if (destination.UsesCostCenter && !detail.CostCenterId.HasValue)
                {
                    if (destination.CostCenterID.HasValue)
                    {
                        detail.CostCenterId = destination.CostCenterID;
                        System.Diagnostics.Debug.WriteLine($"AutoFillCostCenterAsync: Set CostCenterId to {destination.CostCenterID} from destination {destination.DestinationCode}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"AutoFillCostCenterAsync: Warning - Destination {destination.DestinationCode} requires cost center but none is defined");
                    }
                }
                // إذا كانت الجهة لا تتطلب مركز كلفة ولكن كان محدداً من المخزن الافتراضي
                else if (!destination.UsesCostCenter && !detail.CostCenterId.HasValue)
                {
                    // جلب مركز الكلفة الافتراضي من المخزن
                    var warehouse = await _db.Warehouses.FindAsync(detail.WarehouseId);
                    if (warehouse?.DefultCostCenter.HasValue == true)
                    {
                        detail.CostCenterId = warehouse.DefultCostCenter;
                        System.Diagnostics.Debug.WriteLine($"AutoFillCostCenterAsync: Set CostCenterId to {warehouse.DefultCostCenter} from warehouse {warehouse.WarehouseCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoFillCostCenterAsync: Error: {ex.Message}");
            }
        }

        /// <summary>
        /// تعبئة تكلفة الوحدة تلقائياً من رصيد المخزون أو تكلفة الصنف الافتراضية
        /// </summary>
        private async Task AutoFillUnitCostAsync(StoreIssueDetail detail)
        {
            try
            {
                // إذا كانت التكلفة محددة مسبقاً، لا نغيرها
                if (detail.UnitCost > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: UnitCost already set to {detail.UnitCost} for detail {detail.DetailId}");
                    return;
                }

                // البحث عن رصيد المخزون الحالي
                var balance = await _db.InventoryBalances
                    .FirstOrDefaultAsync(x => x.WarehouseID == detail.WarehouseId 
                                           && x.ItemID == detail.ItemId 
                                           && x.BatchID == detail.BatchID);

                if (balance != null && balance.AvgCost > 0)
                {
                    // استخدام متوسط التكلفة من رصيد المخزون
                    detail.UnitCost = balance.AvgCost;
                    System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: Set UnitCost to AvgCost {balance.AvgCost} from inventory balance");
                }
                else
                {
                    // البحث عن تكلفة الصنف الافتراضية
                    var item = await _db.Items.FindAsync(detail.ItemId);
                    if (item != null)
                    {
                        // استخدام آخر سعر شراء أو التكلفة المعيارية
                        if (item.LastPurchasePrice.HasValue && item.LastPurchasePrice > 0)
                        {
                            detail.UnitCost = item.LastPurchasePrice.Value;
                            System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: Set UnitCost to LastPurchasePrice {item.LastPurchasePrice} for item {item.ItemCode}");
                        }
                        else if (item.StandardCost.HasValue && item.StandardCost > 0)
                        {
                            detail.UnitCost = item.StandardCost.Value;
                            System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: Set UnitCost to StandardCost {item.StandardCost} for item {item.ItemCode}");
                        }
                        else
                        {
                            // إذا لم توجد تكلفة، استخدم صفر مع تحذير
                            detail.UnitCost = 0;
                            System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: Warning - No cost found for item {item.ItemCode}, set to 0");
                        }
                    }
                    else
                    {
                        detail.UnitCost = 0;
                        System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: Warning - Item {detail.ItemId} not found, set UnitCost to 0");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoFillUnitCostAsync: Error: {ex.Message}");
                // في حالة الخطأ، استخدم صفر كقيمة افتراضية
                detail.UnitCost = 0;
            }
        }
    }
}
