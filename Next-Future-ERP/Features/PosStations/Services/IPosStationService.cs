using Next_Future_ERP.Features.PosStations.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PosStations.Services
{
    /// <summary>
    /// واجهة خدمة إدارة محطات نقاط البيع
    /// </summary>
    public interface IPosStationService
    {
        /// <summary>
        /// جلب جميع محطات نقاط البيع
        /// </summary>
        Task<List<PosStation>> GetAllAsync(int? companyId = null, int? branchId = null, bool? isActive = null);

        /// <summary>
        /// جلب محطة نقطة بيع بالمعرف
        /// </summary>
        Task<PosStation?> GetByIdAsync(int posId);

        /// <summary>
        /// جلب محطة نقطة بيع بالكود
        /// </summary>
        Task<PosStation?> GetByCodeAsync(string posCode);

        /// <summary>
        /// إضافة محطة نقطة بيع جديدة
        /// </summary>
        Task<PosStation> AddAsync(PosStation station);

        /// <summary>
        /// تحديث محطة نقطة بيع
        /// </summary>
        Task<PosStation> UpdateAsync(PosStation station);

        /// <summary>
        /// حذف محطة نقطة بيع
        /// </summary>
        Task<bool> DeleteAsync(int posId);

        /// <summary>
        /// تغيير حالة محطة نقطة البيع
        /// </summary>
        Task<bool> ToggleActiveAsync(int posId);

        /// <summary>
        /// البحث في محطات نقاط البيع
        /// </summary>
        Task<List<PosStation>> SearchAsync(string searchTerm, int? companyId = null, int? branchId = null);

        /// <summary>
        /// توليد كود نقطة بيع جديد
        /// </summary>
        Task<string> GenerateNextCodeAsync();

        /// <summary>
        /// التحقق من صحة البيانات
        /// </summary>
        Task<bool> ValidateAsync(PosStation station);
    }
}
