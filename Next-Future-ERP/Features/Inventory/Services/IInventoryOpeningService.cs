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
    /// واجهة خدمة الجرد الافتتاحي
    /// </summary>
    public interface IInventoryOpeningService
    {
        #region Header Operations

        /// <summary>
        /// إنشاء مستند جرد افتتاحي جديد
        /// </summary>
        /// <param name="header">بيانات رأس المستند</param>
        /// <returns>المستند المُنشأ</returns>
        Task<InventoryOpeningHeader> CreateHeaderAsync(InventoryOpeningHeader header);

        /// <summary>
        /// تحديث مستند الجرد الافتتاحي
        /// </summary>
        /// <param name="header">بيانات رأس المستند المحدثة</param>
        /// <returns>المستند المحدث</returns>
        Task<InventoryOpeningHeader> UpdateHeaderAsync(InventoryOpeningHeader header);

        /// <summary>
        /// حذف مستند الجرد الافتتاحي
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <returns>نتيجة العملية</returns>
        Task<bool> DeleteHeaderAsync(int docId);

        /// <summary>
        /// الحصول على مستند الجرد الافتتاحي بمعرفه
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <returns>المستند مع تفاصيله</returns>
        Task<InventoryOpeningHeader?> GetHeaderByIdAsync(int docId);

        /// <summary>
        /// الحصول على قائمة مستندات الجرد الافتتاحي
        /// </summary>
        /// <param name="companyId">معرف الشركة</param>
        /// <param name="branchId">معرف الفرع</param>
        /// <param name="dateFrom">من تاريخ</param>
        /// <param name="dateTo">إلى تاريخ</param>
        /// <param name="status">الحالة (اختياري)</param>
        /// <returns>قائمة المستندات</returns>
        Task<IEnumerable<InventoryOpeningHeader>> GetHeadersAsync(int companyId, int branchId, 
            DateTime? dateFrom = null, DateTime? dateTo = null, InventoryOpeningStatus? status = null);

        /// <summary>
        /// اعتماد مستند الجرد الافتتاحي
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <param name="approvedBy">معرف المعتمد</param>
        /// <returns>نتيجة العملية</returns>
        Task<bool> ApproveDocumentAsync(int docId, int approvedBy);

        /// <summary>
        /// إلغاء اعتماد مستند الجرد الافتتاحي
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <returns>نتيجة العملية</returns>
        Task<bool> UnapproveDocumentAsync(int docId);

        #endregion

        #region Detail Operations

        /// <summary>
        /// إضافة سطر تفصيلي جديد
        /// </summary>
        /// <param name="detail">بيانات السطر</param>
        /// <returns>السطر المُضاف</returns>
        Task<InventoryOpeningDetail> AddDetailAsync(InventoryOpeningDetail detail);

        /// <summary>
        /// تحديث سطر تفصيلي
        /// </summary>
        /// <param name="detail">بيانات السطر المحدثة</param>
        /// <returns>السطر المحدث</returns>
        Task<InventoryOpeningDetail> UpdateDetailAsync(InventoryOpeningDetail detail);

        /// <summary>
        /// حذف سطر تفصيلي
        /// </summary>
        /// <param name="lineId">معرف السطر</param>
        /// <returns>نتيجة العملية</returns>
        Task<bool> DeleteDetailAsync(int lineId);

        /// <summary>
        /// الحصول على تفاصيل مستند
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <returns>قائمة التفاصيل</returns>
        Task<IEnumerable<InventoryOpeningDetail>> GetDetailsAsync(int docId);

        /// <summary>
        /// إضافة عدة أسطر تفصيلية
        /// </summary>
        /// <param name="details">قائمة الأسطر</param>
        /// <returns>الأسطر المُضافة</returns>
        Task<IEnumerable<InventoryOpeningDetail>> AddDetailsAsync(IEnumerable<InventoryOpeningDetail> details);

        #endregion

        #region Auto Generation

        /// <summary>
        /// توليد أسطر تلقائية بناءً على مرشحات الترويسة
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <param name="categoryIds">معرفات التصنيفات (اختياري)</param>
        /// <param name="warehouseIds">معرفات المخازن (اختياري)</param>
        /// <param name="activeItemsOnly">الأصناف النشطة فقط</param>
        /// <returns>الأسطر المُولدة</returns>
        Task<IEnumerable<InventoryOpeningDetail>> GenerateAutoDetailsAsync(int docId, 
            int[]? categoryIds = null, int[]? warehouseIds = null, bool activeItemsOnly = true);

        #endregion

        #region Validation

        /// <summary>
        /// التحقق من صحة المستند قبل الاعتماد
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <returns>قائمة الأخطاء (فارغة إذا كان المستند صحيحاً)</returns>
        Task<IEnumerable<string>> ValidateDocumentAsync(int docId);

        /// <summary>
        /// التحقق من عدم تكرار البيانات
        /// </summary>
        /// <param name="docId">معرف المستند</param>
        /// <param name="itemId">معرف الصنف</param>
        /// <param name="warehouseId">معرف المخزن</param>
        /// <param name="batchNo">رقم الدفعة</param>
        /// <param name="serialNo">الرقم التسلسلي</param>
        /// <param name="expiryDate">تاريخ الانتهاء</param>
        /// <param name="excludeLineId">معرف السطر المستثنى من التحقق (للتحديث)</param>
        /// <returns>true إذا كان هناك تكرار</returns>
        Task<bool> CheckDuplicateAsync(int docId, int itemId, int warehouseId, 
            string? batchNo = null, string? serialNo = null, DateTime? expiryDate = null, 
            int? excludeLineId = null);

        #endregion

        #region Lookup Data

        /// <summary>
        /// الحصول على قائمة الأصناف المتاحة
        /// </summary>
        /// <param name="companyId">معرف الشركة</param>
        /// <param name="activeOnly">النشطة فقط</param>
        /// <returns>قائمة الأصناف</returns>
        Task<IEnumerable<Item>> GetAvailableItemsAsync(int companyId, bool activeOnly = true);

        /// <summary>
        /// الحصول على قائمة المخازن المتاحة
        /// </summary>
        /// <param name="companyId">معرف الشركة</param>
        /// <param name="branchId">معرف الفرع</param>
        /// <param name="activeOnly">النشطة فقط</param>
        /// <returns>قائمة المخازن</returns>
        Task<IEnumerable<Warehouse>> GetAvailableWarehousesAsync(int companyId, int branchId, bool activeOnly = true);

        /// <summary>
        /// الحصول على معامل التحويل بين الوحدات
        /// </summary>
        /// <param name="itemId">معرف الصنف</param>
        /// <param name="fromUnitId">من وحدة</param>
        /// <param name="toUnitId">إلى وحدة</param>
        /// <returns>معامل التحويل</returns>
        Task<decimal?> GetUnitConversionFactorAsync(int itemId, int fromUnitId, int toUnitId);

        #endregion

        #region Document Numbering

        /// <summary>
        /// توليد رقم مستند جديد
        /// </summary>
        /// <param name="companyId">معرف الشركة</param>
        /// <param name="branchId">معرف الفرع</param>
        /// <returns>رقم المستند الجديد</returns>
        Task<string> GenerateDocumentNumberAsync(int companyId, int branchId);

        #endregion
    }
}
