using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Next_Future_ERP.Data;
using Next_Future_ERP.Models;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class NumberingService : INumberingService
    {
        private readonly AppDbContext _db;
        public NumberingService(AppDbContext db) => _db = db;

        public async Task<string> GenerateNextAsync(int documentTypeId, int branchId, string defaultPrefix = "")
        {
            // تحديد البادئة من DocumentTypes (أولوية: SequencePrefix ثم DocumentCode ثم defaultPrefix)
            var docType = await _db.DocumentTypes
                                   .Where(d => d.DocumentTypeId == documentTypeId && d.IsActive == true)
                                   .Select(d => new { d.SequencePrefix, d.DocumentCode })
                                   .FirstOrDefaultAsync();

            var prefix =
                !string.IsNullOrWhiteSpace(docType?.SequencePrefix) ? docType!.SequencePrefix! :
                !string.IsNullOrWhiteSpace(docType?.DocumentCode) ? docType!.DocumentCode! :
                defaultPrefix;

            // إن كانت هناك معاملة جارية على نفس DbContext فلا نفتـح معاملة جديدة
            var hasAmbient = _db.Database.CurrentTransaction != null;
            IDbContextTransaction localTx = null;

            try
            {
                if (!hasAmbient)
                    localTx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                // جلب/إنشاء صف التسلسل
                var seq = await _db.DocumentSequences
                    .SingleOrDefaultAsync(s => s.DocumentTypeId == documentTypeId && s.BranchId == branchId);

                if (seq == null)
                {
                    seq = new DocumentSequence
                    {
                        DocumentTypeId = documentTypeId,
                        BranchId = branchId,
                        CurrentNo = 0
                    };
                    _db.DocumentSequences.Add(seq);
                    await _db.SaveChangesAsync();
                }

                // الزيادة والحفظ
                seq.CurrentNo += 1;
                await _db.SaveChangesAsync();

                if (localTx != null) await localTx.CommitAsync();

                return $"{prefix}{seq.CurrentNo:D8}";
            }
            catch
            {
                if (localTx != null) await localTx.RollbackAsync();
                throw;
            }
        }
    }
}
