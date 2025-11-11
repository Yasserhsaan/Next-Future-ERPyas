using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Next_Future_ERP.Features.PrintManagement.Services
{
    public interface IDataSourceExecutor
    {
        Task<Dictionary<string, object>> GetDocumentDataAsync(
            int templateVersionId, int documentTypeId, long documentId,
            int companyId, int? branchId, string? locale);
    }
}

