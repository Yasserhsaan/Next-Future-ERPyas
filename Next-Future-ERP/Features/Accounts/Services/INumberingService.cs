using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public interface INumberingService
    {
        /// <summary>
        /// يولّد ويحجز رقم المستند التالي لنوع مستند/فرع محدد.
        /// يأخذ بادئة افتراضية عند غياب الإعداد في DocumentTypes.
        /// يحترم أي Transaction حالية على نفس DbContext.
        /// </summary>
        Task<string> GenerateNextAsync(int documentTypeId, int branchId, string defaultPrefix = "");
    }
}
