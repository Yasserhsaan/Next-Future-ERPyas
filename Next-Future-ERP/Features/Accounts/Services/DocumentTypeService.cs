// DocumentTypeService.cs
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
    public class DocumentTypeService : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly bool _ownsContext;

        public DocumentTypeService()
        {
            _context = DbContextFactory.Create();
            _ownsContext = true;
        }

        public DocumentTypeService(AppDbContext context)
        {
            _context = context;
            _ownsContext = false;
        }

        public async Task<List<DocumentType>> GetAllAsync()
        {
            try
            {
                return await _context.DocumentTypes
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء جلب أنواع المستندات", ex);
            }
        }

        public async Task<DocumentType> GetByIdAsync(int id)
        {
            try
            {
                return await _context.DocumentTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dt => dt.DocumentTypeId == id);
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء جلب نوع المستند", ex);
            }
        }

        public async Task SaveAsync(DocumentType documentType)
        {
            try
            {
                if (documentType.DocumentTypeId > 0)
                {
                    var existing = await _context.DocumentTypes.FindAsync(documentType.DocumentTypeId);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(documentType);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    if (!documentType.CreatedAt.HasValue)
                        documentType.CreatedAt = DateTime.Now;

                    _context.DocumentTypes.Add(documentType);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء حفظ نوع المستند", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var documentType = await _context.DocumentTypes.FindAsync(id);

                if (documentType != null && !documentType.IsSystem)
                {
                    _context.DocumentTypes.Remove(documentType);
                    await _context.SaveChangesAsync();
                }
                else if (documentType?.IsSystem == true)
                {
                    throw new InvalidOperationException("❌ لا يمكن حذف أنواع المستندات النظامية");
                }
                else
                {
                    throw new InvalidOperationException("⚠️ لم يتم العثور على نوع المستند المطلوب حذفه");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء حذف نوع المستند", ex);
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
