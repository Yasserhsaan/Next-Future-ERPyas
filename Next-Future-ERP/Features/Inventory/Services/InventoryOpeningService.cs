using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Inventory.Models;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Warehouses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Inventory.Services
{
    /// <summary>
    /// تنفيذ خدمة الجرد الافتتاحي
    /// </summary>
    public class InventoryOpeningService : IInventoryOpeningService
    {
        private readonly AppDbContext _context;

        public InventoryOpeningService(AppDbContext context)
        {
            _context = context;
        }

        #region Header Operations

        public async Task<InventoryOpeningHeader> CreateHeaderAsync(InventoryOpeningHeader header)
        {
            // توليد رقم المستند إذا لم يكن محدداً
            if (string.IsNullOrEmpty(header.DocNo))
            {
                header.DocNo = await GenerateDocumentNumberAsync(header.CompanyId, header.BranchId);
            }

            header.CreatedAt = DateTime.Now;
            header.Status = InventoryOpeningStatus.Draft;

            _context.Set<InventoryOpeningHeader>().Add(header);
            await _context.SaveChangesAsync();

            return header;
        }

        public async Task<InventoryOpeningHeader> UpdateHeaderAsync(InventoryOpeningHeader header)
        {
            var existingHeader = await GetHeaderByIdAsync(header.DocID);
            if (existingHeader == null)
                throw new ArgumentException("المستند غير موجود");

            if (existingHeader.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن تعديل مستند معتمد");

            // تحديث الحقول القابلة للتعديل
            existingHeader.DocNo = header.DocNo;
            existingHeader.DocDate = header.DocDate;
            existingHeader.EntryMethod = header.EntryMethod;
            existingHeader.ViewMode = header.ViewMode;
            existingHeader.CostMethod = header.CostMethod;
            existingHeader.WeightedAvgScope = header.WeightedAvgScope;
            existingHeader.UseExpiry = header.UseExpiry;
            existingHeader.UseBatch = header.UseBatch;
            existingHeader.UseSerial = header.UseSerial;
            existingHeader.Notes = header.Notes;
            existingHeader.ModifiedBy = header.ModifiedBy;
            existingHeader.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingHeader;
        }

        public async Task<bool> DeleteHeaderAsync(int docId)
        {
            var header = await GetHeaderByIdAsync(docId);
            if (header == null)
                return false;

            if (header.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن حذف مستند معتمد");

            // حذف التفاصيل أولاً
            var details = await GetDetailsAsync(docId);
            _context.Set<InventoryOpeningDetail>().RemoveRange(details);

            // حذف الرأس
            _context.Set<InventoryOpeningHeader>().Remove(header);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InventoryOpeningHeader?> GetHeaderByIdAsync(int docId)
        {
            return await _context.Set<InventoryOpeningHeader>()
                .Include(h => h.Details)
                    .ThenInclude(d => d.Item)
                .Include(h => h.Details)
                    .ThenInclude(d => d.Unit)
                .Include(h => h.Details)
                    .ThenInclude(d => d.NumericUnit)
                .Include(h => h.Details)
                    .ThenInclude(d => d.Warehouse)
                .FirstOrDefaultAsync(h => h.DocID == docId);
        }

        public async Task<IEnumerable<InventoryOpeningHeader>> GetHeadersAsync(int companyId, int branchId, 
            DateTime? dateFrom = null, DateTime? dateTo = null, InventoryOpeningStatus? status = null)
        {
            var query = _context.Set<InventoryOpeningHeader>()
                .Where(h => h.CompanyId == companyId && h.BranchId == branchId);

            if (dateFrom.HasValue)
                query = query.Where(h => h.DocDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(h => h.DocDate <= dateTo.Value);

            if (status.HasValue)
                query = query.Where(h => h.Status == status.Value);

            return await query
                .OrderByDescending(h => h.DocDate)
                .ThenByDescending(h => h.DocID)
                .ToListAsync();
        }

        public async Task<bool> ApproveDocumentAsync(int docId, int approvedBy)
        {
            var header = await GetHeaderByIdAsync(docId);
            if (header == null)
                return false;

            if (header.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("المستند معتمد بالفعل");

            // التحقق من صحة المستند
            var validationErrors = await ValidateDocumentAsync(docId);
            if (validationErrors.Any())
                throw new InvalidOperationException($"لا يمكن اعتماد المستند: {string.Join(", ", validationErrors)}");

            header.Status = InventoryOpeningStatus.Approved;
            header.ApprovedBy = approvedBy;
            header.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnapproveDocumentAsync(int docId)
        {
            var header = await GetHeaderByIdAsync(docId);
            if (header == null)
                return false;

            if (header.Status != InventoryOpeningStatus.Approved)
                return false;

            header.Status = InventoryOpeningStatus.Draft;
            header.ApprovedBy = null;
            header.ApprovedAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Detail Operations

        public async Task<InventoryOpeningDetail> AddDetailAsync(InventoryOpeningDetail detail)
        {
            // التحقق من وجود المستند وإمكانية التعديل
            var header = await GetHeaderByIdAsync(detail.DocID);
            if (header == null)
                throw new ArgumentException("المستند غير موجود");

            if (header.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن إضافة أسطر لمستند معتمد");

            // التحقق من عدم التكرار
            var isDuplicate = await CheckDuplicateAsync(detail.DocID, detail.ItemID, detail.WarehouseId, 
                detail.BatchNo, detail.SerialNo, detail.ExpiryDate);
            
            if (isDuplicate)
                throw new InvalidOperationException("يوجد سطر مماثل بالفعل في المستند");

            // التحقق من صحة البيانات
            if (!detail.IsValid())
                throw new ArgumentException("بيانات السطر غير صحيحة");

            _context.Set<InventoryOpeningDetail>().Add(detail);
            await _context.SaveChangesAsync();

            return detail;
        }

        public async Task<InventoryOpeningDetail> UpdateDetailAsync(InventoryOpeningDetail detail)
        {
            var existingDetail = await _context.Set<InventoryOpeningDetail>()
                .FirstOrDefaultAsync(d => d.LineID == detail.LineID);

            if (existingDetail == null)
                throw new ArgumentException("السطر غير موجود");

            // التحقق من إمكانية التعديل
            var header = await GetHeaderByIdAsync(existingDetail.DocID);
            if (header?.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن تعديل أسطر مستند معتمد");

            // التحقق من عدم التكرار (باستثناء السطر الحالي)
            var isDuplicate = await CheckDuplicateAsync(detail.DocID, detail.ItemID, detail.WarehouseId, 
                detail.BatchNo, detail.SerialNo, detail.ExpiryDate, detail.LineID);
            
            if (isDuplicate)
                throw new InvalidOperationException("يوجد سطر مماثل بالفعل في المستند");

            // تحديث البيانات
            existingDetail.ItemID = detail.ItemID;
            existingDetail.UnitID = detail.UnitID;
            existingDetail.NumericUnitID = detail.NumericUnitID;
            existingDetail.NumericQty = detail.NumericQty;
            existingDetail.Qty = detail.Qty;
            existingDetail.WarehouseId = detail.WarehouseId;
            existingDetail.ExpiryDate = detail.ExpiryDate;
            existingDetail.BatchNo = detail.BatchNo;
            existingDetail.SerialNo = detail.SerialNo;
            existingDetail.InitialUnitCost = detail.InitialUnitCost;
            existingDetail.CurrencyId = detail.CurrencyId;
            existingDetail.LineNotes = detail.LineNotes;

            if (!existingDetail.IsValid())
                throw new ArgumentException("بيانات السطر غير صحيحة");

            await _context.SaveChangesAsync();
            return existingDetail;
        }

        public async Task<bool> DeleteDetailAsync(int lineId)
        {
            var detail = await _context.Set<InventoryOpeningDetail>()
                .Include(d => d.Header)
                .FirstOrDefaultAsync(d => d.LineID == lineId);

            if (detail == null)
                return false;

            if (detail.Header.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن حذف أسطر من مستند معتمد");

            _context.Set<InventoryOpeningDetail>().Remove(detail);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InventoryOpeningDetail>> GetDetailsAsync(int docId)
        {
            return await _context.Set<InventoryOpeningDetail>()
                .Include(d => d.Item)
                .Include(d => d.Unit)
                .Include(d => d.NumericUnit)
                .Include(d => d.Warehouse)
                .Where(d => d.DocID == docId)
                .OrderBy(d => d.LineID)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryOpeningDetail>> AddDetailsAsync(IEnumerable<InventoryOpeningDetail> details)
        {
            var detailsList = details.ToList();
            if (!detailsList.Any())
                return detailsList;

            // التحقق من وجود المستند
            var docId = detailsList.First().DocID;
            var header = await GetHeaderByIdAsync(docId);
            if (header == null)
                throw new ArgumentException("المستند غير موجود");

            if (header.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن إضافة أسطر لمستند معتمد");

            // التحقق من صحة جميع الأسطر
            foreach (var detail in detailsList)
            {
                if (!detail.IsValid())
                    throw new ArgumentException($"بيانات السطر رقم {detail.LineID} غير صحيحة");

                var isDuplicate = await CheckDuplicateAsync(detail.DocID, detail.ItemID, detail.WarehouseId, 
                    detail.BatchNo, detail.SerialNo, detail.ExpiryDate);
                
                if (isDuplicate)
                    throw new InvalidOperationException($"السطر {detail.ItemInfo} - {detail.WarehouseInfo} مكرر");
            }

            _context.Set<InventoryOpeningDetail>().AddRange(detailsList);
            await _context.SaveChangesAsync();

            return detailsList;
        }

        #endregion

        #region Auto Generation

        public async Task<IEnumerable<InventoryOpeningDetail>> GenerateAutoDetailsAsync(int docId, 
            int[]? categoryIds = null, int[]? warehouseIds = null, bool activeItemsOnly = true)
        {
            var header = await GetHeaderByIdAsync(docId);
            if (header == null)
                throw new ArgumentException("المستند غير موجود");

            if (header.Status == InventoryOpeningStatus.Approved)
                throw new InvalidOperationException("لا يمكن توليد أسطر لمستند معتمد");

            // الحصول على الأصناف
            var itemsQuery = _context.Set<Item>()
                .Where(i => (i.IsActive.HasValue && i.IsActive.Value) || !activeItemsOnly);

            if (categoryIds?.Any() == true)
                itemsQuery = itemsQuery.Where(i => i.CategoryID.HasValue && categoryIds.Contains(i.CategoryID.Value));

            var items = await itemsQuery.ToListAsync();

            // الحصول على المخازن
            var warehousesQuery = _context.Set<Warehouse>()
                .Where(w => w.CompanyId == header.CompanyId && w.BranshId == header.BranchId);

            if (warehouseIds?.Any() == true)
                warehousesQuery = warehousesQuery.Where(w => warehouseIds.Contains(w.WarehouseID));

            var warehouses = await warehousesQuery.ToListAsync();

            // توليد الأسطر
            var details = new List<InventoryOpeningDetail>();

            foreach (var item in items)
            {
                foreach (var warehouse in warehouses)
                {
                    var detail = new InventoryOpeningDetail
                    {
                        DocID = docId,
                        ItemID = item.ItemID,
                        UnitID = item.UnitID ?? 0, // يجب التأكد من وجود وحدة أساسية
                        WarehouseId = warehouse.WarehouseID,
                        Qty = 0, // المستخدم سيدخل الكمية
                        InitialUnitCost = item.StandardCost
                    };

                    details.Add(detail);
                }
            }

            return details;
        }

        #endregion

        #region Validation

        public async Task<IEnumerable<string>> ValidateDocumentAsync(int docId)
        {
            var errors = new List<string>();
            var header = await GetHeaderByIdAsync(docId);

            if (header == null)
            {
                errors.Add("المستند غير موجود");
                return errors;
            }

            // التحقق من وجود تفاصيل
            if (!header.Details.Any())
            {
                errors.Add("يجب إضافة تفاصيل للمستند");
            }

            // التحقق من صحة التفاصيل
            foreach (var detail in header.Details)
            {
                if (!detail.IsValid())
                {
                    errors.Add($"بيانات السطر {detail.LineID} غير صحيحة");
                }

                if (detail.Qty <= 0)
                {
                    errors.Add($"الكمية في السطر {detail.LineID} يجب أن تكون أكبر من الصفر");
                }

                if (header.UseExpiry && detail.ExpiryDate == null)
                {
                    errors.Add($"تاريخ الانتهاء مطلوب في السطر {detail.LineID}");
                }

                if (header.UseBatch && string.IsNullOrWhiteSpace(detail.BatchNo))
                {
                    errors.Add($"رقم الدفعة مطلوب في السطر {detail.LineID}");
                }

                if (header.UseSerial && string.IsNullOrWhiteSpace(detail.SerialNo))
                {
                    errors.Add($"الرقم التسلسلي مطلوب في السطر {detail.LineID}");
                }
            }

            return errors;
        }

        public async Task<bool> CheckDuplicateAsync(int docId, int itemId, int warehouseId, 
            string? batchNo = null, string? serialNo = null, DateTime? expiryDate = null, 
            int? excludeLineId = null)
        {
            var query = _context.Set<InventoryOpeningDetail>()
                .Where(d => d.DocID == docId && d.ItemID == itemId && d.WarehouseId == warehouseId);

            if (excludeLineId.HasValue)
                query = query.Where(d => d.LineID != excludeLineId.Value);

            // التحقق من الدفعة
            if (!string.IsNullOrWhiteSpace(batchNo))
            {
                query = query.Where(d => d.BatchNo == batchNo);
                
                if (expiryDate.HasValue)
                    query = query.Where(d => d.ExpiryDate == expiryDate.Value);
            }

            // التحقق من الرقم التسلسلي
            if (!string.IsNullOrWhiteSpace(serialNo))
            {
                query = query.Where(d => d.SerialNo == serialNo);
            }

            return await query.AnyAsync();
        }

        #endregion

        #region Lookup Data

        public async Task<IEnumerable<Item>> GetAvailableItemsAsync(int companyId, bool activeOnly = true)
        {
            var query = _context.Set<Item>().AsQueryable();

            if (activeOnly)
                query = query.Where(i => i.IsActive.HasValue && i.IsActive.Value);

            return await query
                .Include(i => i.Category)
                .Include(i => i.Unit)
                .OrderBy(i => i.ItemCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Warehouse>> GetAvailableWarehousesAsync(int companyId, int branchId, bool activeOnly = true)
        {
            var query = _context.Set<Warehouse>()
                .Where(w => w.CompanyId == companyId && w.BranshId == branchId);

            if (activeOnly)
                query = query.Where(w => w.IsActive.HasValue && w.IsActive.Value);

            return await query
                .OrderBy(w => w.WarehouseCode)
                .ToListAsync();
        }

        public async Task<decimal?> GetUnitConversionFactorAsync(int itemId, int fromUnitId, int toUnitId)
        {
            // هذه دالة مبسطة - يجب تنفيذها بناءً على جداول تحويل الوحدات
            if (fromUnitId == toUnitId)
                return 1;

            // TODO: تنفيذ منطق تحويل الوحدات من جداول النظام
            return null;
        }

        #endregion

        #region Document Numbering

        public async Task<string> GenerateDocumentNumberAsync(int companyId, int branchId)
        {
            var year = DateTime.Now.Year;
            var prefix = $"IO-{year}-";
            
            var lastDoc = await _context.Set<InventoryOpeningHeader>()
                .Where(h => h.CompanyId == companyId && h.BranchId == branchId && h.DocNo.StartsWith(prefix))
                .OrderByDescending(h => h.DocNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastDoc != null && lastDoc.DocNo.Length > prefix.Length)
            {
                var numberPart = lastDoc.DocNo.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }

        #endregion
    }
}
