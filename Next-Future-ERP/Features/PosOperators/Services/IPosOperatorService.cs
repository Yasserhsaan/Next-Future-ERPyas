using Next_Future_ERP.Features.PosOperators.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PosOperators.Services
{
    /// <summary>
    /// واجهة خدمة إدارة مشغلي نقاط البيع
    /// </summary>
    public interface IPosOperatorService
    {
        /// <summary>
        /// جلب جميع مشغلي نقاط البيع
        /// </summary>
        Task<List<PosOperator>> GetAllAsync(int? companyId = null, int? branchId = null, bool? isActive = null);
        
        /// <summary>
        /// جلب مشغل نقطة بيع بالمعرف
        /// </summary>
        Task<PosOperator?> GetByIdAsync(int operatorId);
        
        /// <summary>
        /// جلب مشغلي نقطة بيع محددة
        /// </summary>
        Task<List<PosOperator>> GetByPosIdAsync(int posId);
        
        /// <summary>
        /// جلب مشغلي مستخدم محدد
        /// </summary>
        Task<List<PosOperator>> GetByUserIdAsync(int userId);
        
        /// <summary>
        /// إضافة مشغل نقطة بيع جديد
        /// </summary>
        Task<PosOperator> AddAsync(PosOperator posOperator);
        
        /// <summary>
        /// تحديث مشغل نقطة بيع
        /// </summary>
        Task<PosOperator> UpdateAsync(PosOperator posOperator);
        
        /// <summary>
        /// حذف مشغل نقطة بيع
        /// </summary>
        Task<bool> DeleteAsync(int operatorId);
        
        /// <summary>
        /// تغيير حالة مشغل نقطة البيع
        /// </summary>
        Task<bool> ToggleActiveAsync(int operatorId);
        
        /// <summary>
        /// البحث في مشغلي نقاط البيع
        /// </summary>
        Task<List<PosOperator>> SearchAsync(string searchTerm, int? companyId = null, int? branchId = null);
        
        /// <summary>
        /// التحقق من صحة البيانات
        /// </summary>
        Task<bool> ValidateAsync(PosOperator posOperator);
        
        /// <summary>
        /// تعيين مشغل رئيسي لنقطة بيع
        /// </summary>
        Task<bool> SetPrimaryOperatorAsync(int posId, int operatorId);
    }
}
