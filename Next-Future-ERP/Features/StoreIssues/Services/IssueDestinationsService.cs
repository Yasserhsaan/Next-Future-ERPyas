using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.StoreIssues.Models;

namespace Next_Future_ERP.Features.StoreIssues.Services
{
    public class IssueDestinationsService : IIssueDestinationsService
    {
        private readonly AppDbContext _db;

        public IssueDestinationsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<IssueDestination>> GetAllAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.GetAllAsync: Starting");
                var result = await _db.IssueDestinations
                    .Include(x => x.Account)
                    .Include(x => x.CostCenter)
                    .AsNoTracking()
                    .OrderBy(x => x.DestinationCode)
                    .ToListAsync();
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.GetAllAsync: Found {result.Count} destinations");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.GetAllAsync: Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<IssueDestination>> GetAllAsync(string? searchText)
        {
            var query = _db.IssueDestinations
                .Include(x => x.Account)
                .Include(x => x.CostCenter)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(x =>
                    x.DestinationCode.Contains(searchText) ||
                    x.DestinationName.Contains(searchText) ||
                    x.DestinationTypeText.Contains(searchText) ||
                    (x.Account != null && x.Account.AccountNameAr.Contains(searchText)) ||
                    (x.CostCenter != null && x.CostCenter.CostCenterName.Contains(searchText)));
            }

            return await query
                .OrderBy(x => x.DestinationCode)
                .ToListAsync();
        }

        public async Task<IssueDestination?> GetByIdAsync(int id)
        {
            return await _db.IssueDestinations
                .Include(x => x.Account)
                .Include(x => x.CostCenter)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DestinationID == id);
        }

        public async Task<int> AddAsync(IssueDestination model)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.AddAsync: Starting add operation");
                
                Validate(model);
                
                // استخدام Execution Strategy للتعامل مع المعاملات
                var strategy = _db.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        await _db.IssueDestinations.AddAsync(model);
                        await _db.SaveChangesAsync();
                        await tx.CommitAsync();
                        
                        System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.AddAsync: Successfully added destination with ID: {model.DestinationID}");
                        return model.DestinationID;
                    }
                    catch (DbUpdateException ex)
                    {
                        await tx.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.AddAsync: Database update error: {ex.Message}");
                        var root = ex.GetBaseException().Message;
                        throw new InvalidOperationException($"فشل الحفظ: {root}", ex);
                    }
                });
            }
            catch (InvalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.AddAsync: Validation error occurred");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.AddAsync: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ غير متوقع أثناء إضافة جهة الصرف: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(IssueDestination model)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.UpdateAsync: Starting update operation");
                
                if (model.DestinationID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.UpdateAsync: Invalid DestinationID: {model.DestinationID}");
                    throw new InvalidOperationException("لا يمكن التعديل بدون معرف جهة الصرف.");
                }

                Validate(model);

                // استخدام Execution Strategy للتعامل مع المعاملات
                var strategy = _db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        await _db.IssueDestinations
                            .Where(x => x.DestinationID == model.DestinationID)
                            .ExecuteUpdateAsync(set => set
                                .SetProperty(p => p.DestinationName, model.DestinationName)
                                .SetProperty(p => p.DestinationType, model.DestinationType)
                                .SetProperty(p => p.AccountID, model.AccountID)
                                .SetProperty(p => p.CostCenterID, model.CostCenterID)
                                .SetProperty(p => p.UsesCostCenter, model.UsesCostCenter)
                                .SetProperty(p => p.AllowAccountOverride, model.AllowAccountOverride)
                                .SetProperty(p => p.AllowLineOverride, model.AllowLineOverride)
                                .SetProperty(p => p.IsActive, model.IsActive)
                                .SetProperty(p => p.Description, model.Description)
                                .SetProperty(p => p.ModifiedAt, DateTime.Now)
                                .SetProperty(p => p.ModifiedBy, model.ModifiedBy)
                            );

                        await tx.CommitAsync();
                        System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.UpdateAsync: Successfully updated destination with ID: {model.DestinationID}");
                    }
                    catch (DbUpdateException ex)
                    {
                        await tx.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.UpdateAsync: Database update error: {ex.Message}");
                        var root = ex.GetBaseException().Message;
                        throw new InvalidOperationException($"فشل التحديث: {root}", ex);
                    }
                });
            }
            catch (InvalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.UpdateAsync: Validation error occurred");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.UpdateAsync: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ غير متوقع أثناء تحديث جهة الصرف: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.DeleteAsync: Starting delete operation for ID: {id}");
                
                if (id <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.DeleteAsync: Invalid ID: {id}");
                    throw new InvalidOperationException("معرف جهة الصرف غير صحيح.");
                }

                // استخدام Execution Strategy للتعامل مع المعاملات
                var strategy = _db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        var deleted = await _db.IssueDestinations
                            .Where(x => x.DestinationID == id)
                            .ExecuteDeleteAsync();

                        if (deleted == 0)
                        {
                            throw new InvalidOperationException("جهة الصرف غير موجودة.");
                        }

                        await tx.CommitAsync();
                        System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.DeleteAsync: Successfully deleted destination with ID: {id}");
                    }
                    catch (DbUpdateException ex)
                    {
                        await tx.RollbackAsync();
                        System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.DeleteAsync: Database update error: {ex.Message}");
                        var root = ex.GetBaseException().Message;
                        throw new InvalidOperationException($"فشل الحذف: {root}", ex);
                    }
                });
            }
            catch (InvalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.DeleteAsync: Validation error occurred");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.DeleteAsync: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ غير متوقع أثناء حذف جهة الصرف: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsAsync(int companyId, int branchId, string destinationCode)
        {
            return await _db.IssueDestinations
                .AnyAsync(x => x.CompanyID == companyId && 
                              x.BranchID == branchId && 
                              x.DestinationCode == destinationCode);
        }

        public async Task<string> GenerateNextCodeAsync(int companyId, int branchId)
        {
            // صيغة: DEST-001, DEST-002, ...
            var lastCode = await _db.IssueDestinations
                .Where(x => x.CompanyID == companyId && x.BranchID == branchId)
                .OrderByDescending(x => x.DestinationID)
                .Select(x => x.DestinationCode)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.StartsWith("DEST-"))
            {
                var parts = lastCode.Split('-');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var seq))
                {
                    nextSeq = seq + 1;
                }
            }

            return $"DEST-{nextSeq:D3}";
        }

        public async Task<IEnumerable<IssueDestination>> GetActiveAsync()
        {
            return await _db.IssueDestinations
                .Include(x => x.Account)
                .Include(x => x.CostCenter)
                .Where(x => x.IsActive)
                .AsNoTracking()
                .OrderBy(x => x.DestinationCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<IssueDestination>> GetByTypeAsync(char destinationType)
        {
            return await _db.IssueDestinations
                .Include(x => x.Account)
                .Include(x => x.CostCenter)
                .Where(x => x.DestinationType == destinationType && x.IsActive)
                .AsNoTracking()
                .OrderBy(x => x.DestinationCode)
                .ToListAsync();
        }

        private static void Validate(IssueDestination model)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.Validate: Starting validation");
                
                if (model == null)
                {
                    System.Diagnostics.Debug.WriteLine("IssueDestinationsService.Validate: model is null");
                    throw new InvalidOperationException("بيانات جهة الصرف مطلوبة.");
                }

                if (model.CompanyID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.Validate: Invalid CompanyID: {model.CompanyID}");
                    throw new InvalidOperationException("معرف الشركة مطلوب.");
                }

                if (model.BranchID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.Validate: Invalid BranchID: {model.BranchID}");
                    throw new InvalidOperationException("معرف الفرع مطلوب.");
                }

                if (string.IsNullOrWhiteSpace(model.DestinationCode))
                {
                    System.Diagnostics.Debug.WriteLine("IssueDestinationsService.Validate: DestinationCode is null or empty");
                    throw new InvalidOperationException("كود جهة الصرف مطلوب.");
                }

                if (string.IsNullOrWhiteSpace(model.DestinationName))
                {
                    System.Diagnostics.Debug.WriteLine("IssueDestinationsService.Validate: DestinationName is null or empty");
                    throw new InvalidOperationException("اسم جهة الصرف مطلوب.");
                }

                if (model.DestinationType != 'E' && model.DestinationType != 'P' && 
                    model.DestinationType != 'C' && model.DestinationType != 'S' && 
                    model.DestinationType != 'A' && model.DestinationType != 'O')
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.Validate: Invalid DestinationType: {model.DestinationType}");
                    throw new InvalidOperationException("نوع جهة الصرف غير صحيح.");
                }

                // التحقق من الحساب المطلوب حسب النوع
                if ((model.DestinationType == 'E' || model.DestinationType == 'P' || model.DestinationType == 'C') && 
                    model.AccountID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.Validate: AccountID required for type {model.DestinationType}");
                    throw new InvalidOperationException("الحساب مطلوب لهذا النوع من جهة الصرف.");
                }

                // التحقق من مركز الكلفة إذا كان مطلوباً
                if (model.UsesCostCenter && model.CostCenterID <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("IssueDestinationsService.Validate: CostCenterID required when UsesCostCenter is true");
                    throw new InvalidOperationException("مركز الكلفة مطلوب عند تفعيل استخدام مركز الكلفة.");
                }

                System.Diagnostics.Debug.WriteLine("IssueDestinationsService.Validate: Validation completed successfully");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IssueDestinationsService.Validate: Unexpected error: {ex.Message}");
                throw new InvalidOperationException($"خطأ في التحقق من صحة البيانات: {ex.Message}");
            }
        }
    }
}
