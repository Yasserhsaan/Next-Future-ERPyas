using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Data.Factories;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Accounts.Services
{
    public class GeneralJournalService : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public GeneralJournalService()
        {
            _context = DbContextFactory.Create(); // استخدام مثيل جديد لكل عملية
            _ownsContext = true;
        }

        public GeneralJournalService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

        public async Task<GeneralJournalEntry> GetByCompanyAndBranchAsync(int companyId, int branchId)
        {
            try
            {
                return await _context.GeneralJournalEntries
                    .Include(j => j.Details)
                    .FirstOrDefaultAsync(j => j.CompanyId == companyId && j.BranchId == branchId);
            }
            catch (Exception ex)
            {
                throw new Exception("❌ حدث خطأ أثناء تحميل القيد اليومية", ex);
            }
        }

        public async Task SaveAsync(GeneralJournalEntry journalEntry)
        {
            try
            {
                if (journalEntry.JournalEntryId > 0)
                {
                    var existing = await _context.GeneralJournalEntries.FindAsync(journalEntry.JournalEntryId);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(journalEntry);
                    }
                }
                else
                {
                    _context.GeneralJournalEntries.Add(journalEntry);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ حدث خطأ أثناء حفظ القيد", ex);
            }
        }

        public async Task<List<DocumentType>> GetDocumentTypesAsync()
        {
            try
            {
                return await _context.DocumentTypes
                    .Where(dt => dt.IsActive)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ حدث خطأ أثناء تحميل أنواع المستندات", ex);
            }
        }

       

        public void Dispose()
        {
            if (_ownsContext)
            {
                _context?.Dispose();
            }
        }
    }
}
