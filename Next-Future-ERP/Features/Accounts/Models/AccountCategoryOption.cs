// Models/AccountCategoryOption.cs
using Microsoft.EntityFrameworkCore;

namespace Next_Future_ERP.Models
{


    [Keyless]
    public class AccountCategoryOption
    {
        public string CategoryKey { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public string CategoryNameAr { get; set; } = string.Empty;
        public string? CategoryType { get; set; }

        public string Display => $"{CategoryKey} - {CategoryNameAr} / {CategoryNameEn}";
    }

}
