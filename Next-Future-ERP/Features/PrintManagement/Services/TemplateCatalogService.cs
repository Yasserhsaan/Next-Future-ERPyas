using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة كتالوج القوالب - إدارة البحث والاستعلام عن قوالب الطباعة
    /// </summary>
    public class TemplateCatalogService : ITemplateCatalogService
    {
        private readonly AppDbContext _context;

        public TemplateCatalogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TemplateInfo>> GetTemplatesAsync(
            int? companyId = null,
            int? branchId = null,
            int? documentTypeId = null,
            string? locale = null,
            string? engine = null,
            bool? isActive = null,
            bool? isDefault = null)
        {
            try
            {
                // تحميل القوالب مع ربطها بأنواع المستندات من قاعدة البيانات - نفس منهج القبض والصرف
                var templates = await GetSampleTemplatesAsync(companyId, branchId, documentTypeId);
                
                // تطبيق فلاتر إضافية
                if (!string.IsNullOrEmpty(locale))
                    templates = templates.Where(t => t.Locale == locale).ToList();
                
                if (!string.IsNullOrEmpty(engine))
                    templates = templates.Where(t => t.Engine == engine).ToList();
                
                if (isActive.HasValue)
                    templates = templates.Where(t => t.Active == isActive.Value).ToList();
                
                if (isDefault.HasValue)
                    templates = templates.Where(t => t.IsDefault == isDefault.Value).ToList();

                return templates;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع القوالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TemplateInfo>();
            }
        }

        public Task<Dictionary<string, int>> GetTemplateStatsAsync(int? companyId = null)
        {
            try
            {
                // إرجاع إحصائيات تجريبية مباشرة
                return Task.FromResult(GetSampleStats());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الإحصائيات:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult(new Dictionary<string, int>());
            }
        }

        private async Task<List<TemplateInfo>> GetSampleTemplatesAsync(
            int? companyId = null,
            int? branchId = null,
            int? documentTypeId = null)
        {
            var templates = new List<TemplateInfo>();

            try
            {
                // تحميل أنواع المستندات من قاعدة البيانات - نفس المصدر المستخدم في القبض والصرف
                using var documentTypeService = new Features.Accounts.Services.DocumentTypeService();
                var documentTypes = await documentTypeService.GetAllAsync();
                
                // تصفية أنواع المستندات النشطة
                var activeDocTypes = documentTypes.Where(dt => dt.IsActive).ToList();
                
                // إذا تم تحديد نوع مستند معين، فلترة النتائج
                if (documentTypeId.HasValue)
                {
                    activeDocTypes = activeDocTypes.Where(dt => dt.DocumentTypeId == documentTypeId.Value).ToList();
                }

                // إنشاء قوالب تجريبية بناء على أنواع المستندات الفعلية
                int templateId = 1;
                foreach (var docType in activeDocTypes)
                {
                    templates.Add(new TemplateInfo
                    {
                        TemplateId = templateId++,
                        Name = $"قالب {docType.DocumentNameAr} الافتراضي",
                        DocumentTypeName = docType.DocumentNameAr ?? docType.DocumentNameEn ?? "غير محدد",
                        CompanyName = companyId == 1 ? "شركة المستقبل التالي" : "شركة التطوير",
                        BranchName = GetBranchName(branchId),
                        Locale = "ar-SA",
                        Engine = "Liquid",
                        Active = true,
                        IsDefault = templateId == 2, // أول قالب افتراضي
                        ActiveVersionNo = 1,
                        Status = "نشط"
                    });
                }

                // إضافة قوالب إضافية إذا لم توجد أي أنواع مستندات
                if (!templates.Any())
                {
                    templates.AddRange(GetFallbackTemplates(companyId, branchId));
                }
            }
            catch (Exception)
            {
                // في حالة الخطأ، إرجاع قوالب احتياطية
                templates.AddRange(GetFallbackTemplates(companyId, branchId));
            }

            return templates;
        }

        private string GetBranchName(int? branchId)
        {
            return branchId switch
            {
                1 => "الفرع الرئيسي",
                2 => "فرع الدمام", 
                3 => "فرع جدة",
                null => "جميع الفروع",
                _ => $"فرع {branchId}"
            };
        }

        private List<TemplateInfo> GetFallbackTemplates(int? companyId, int? branchId)
        {
            return new List<TemplateInfo>
            {
                new TemplateInfo
                {
                    TemplateId = 1,
                    Name = "قالب سند قبض افتراضي",
                    DocumentTypeName = "سند قبض",
                    CompanyName = companyId == 1 ? "شركة المستقبل التالي" : "شركة التطوير",
                    BranchName = GetBranchName(branchId),
                    Locale = "ar-SA",
                    Engine = "Liquid",
                    Active = true,
                    IsDefault = true,
                    ActiveVersionNo = 1,
                    Status = "نشط"
                },
                new TemplateInfo
                {
                    TemplateId = 2,
                    Name = "قالب سند دفع افتراضي",
                    DocumentTypeName = "سند دفع",
                    CompanyName = companyId == 1 ? "شركة المستقبل التالي" : "شركة التطوير",
                    BranchName = GetBranchName(branchId),
                    Locale = "ar-SA",
                    Engine = "Liquid",
                    Active = true,
                    IsDefault = false,
                    ActiveVersionNo = 1,
                    Status = "نشط"
                }
            };
        }

        private Dictionary<string, int> GetSampleStats()
        {
            return new Dictionary<string, int>
            {
                ["المجموع"] = 3,
                ["النشطة"] = 3,
                ["المعطلة"] = 0,
                ["الافتراضية"] = 1,
                ["مرات الطباعة"] = 290
            };
        }

        public async Task<PrintTemplate?> GetTemplateByIdAsync(int templateId)
        {
            try
            {
                return await _context.PrintTemplates
                    .Include(t => t.Versions)
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<PrintTemplate> CreateTemplateAsync(PrintTemplate template)
        {
            try
            {
                template.CreatedAt = DateTime.UtcNow;
                _context.PrintTemplates.Add(template);
                await _context.SaveChangesAsync();
                return template;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إنشاء القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<PrintTemplate> UpdateTemplateAsync(PrintTemplate template)
        {
            try
            {
                template.UpdatedAt = DateTime.UtcNow;
                _context.PrintTemplates.Update(template);
                await _context.SaveChangesAsync();
                return template;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحديث القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> ToggleActiveAsync(int templateId, bool isActive)
        {
            try
            {
                var template = await _context.PrintTemplates.FindAsync(templateId);
                if (template == null) return false;

                template.Active = isActive;
                template.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تغيير حالة القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> SetDefaultAsync(int templateId)
        {
            try
            {
                var template = await _context.PrintTemplates.FindAsync(templateId);
                if (template == null) return false;

                // إلغاء الافتراضية من جميع القوالب الأخرى لنفس نوع المستند
                var otherTemplates = await _context.PrintTemplates
                    .Where(t => t.CompanyId == template.CompanyId && 
                               t.BranchId == template.BranchId && 
                               t.DocumentTypeId == template.DocumentTypeId &&
                               t.TemplateId != templateId)
                    .ToListAsync();

                foreach (var otherTemplate in otherTemplates)
                {
                    otherTemplate.IsDefault = false;
                    otherTemplate.UpdatedAt = DateTime.UtcNow;
                }

                // تعيين القالب المحدد كافتراضي
                template.IsDefault = true;
                template.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تعيين القالب الافتراضي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UnsetDefaultAsync(int templateId)
        {
            try
            {
                var template = await _context.PrintTemplates.FindAsync(templateId);
                if (template == null) return false;

                template.IsDefault = false;
                template.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إلغاء القالب الافتراضي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> CanSetDefaultAsync(int templateId)
        {
            try
            {
                var template = await _context.PrintTemplates.FindAsync(templateId);
                return template != null && template.Active;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PrintTemplate> DuplicateTemplateAsync(int sourceTemplateId, string newName)
        {
            try
            {
                var sourceTemplate = await _context.PrintTemplates
                    .Include(t => t.Versions)
                    .FirstOrDefaultAsync(t => t.TemplateId == sourceTemplateId);

                if (sourceTemplate == null)
                    throw new ArgumentException("القالب المصدري غير موجود");

                var duplicatedTemplate = new PrintTemplate
                {
                    CompanyId = sourceTemplate.CompanyId,
                    BranchId = sourceTemplate.BranchId,
                    DocumentTypeId = sourceTemplate.DocumentTypeId,
                    Name = newName,
                    Engine = sourceTemplate.Engine,
                    PaperSize = sourceTemplate.PaperSize,
                    Orientation = sourceTemplate.Orientation,
                    Locale = sourceTemplate.Locale,
                    IsDefault = false, // القالب المكرر ليس افتراضياً
                    Active = true,
                    CreatedBy = sourceTemplate.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PrintTemplates.Add(duplicatedTemplate);
                await _context.SaveChangesAsync();

                return duplicatedTemplate;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في نسخ القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var template = await _context.PrintTemplates.FindAsync(templateId);
                if (template == null) return false;

                // التحقق من وجود قوالب أخرى لنفس نوع المستند
                var otherTemplates = await _context.PrintTemplates
                    .Where(t => t.CompanyId == template.CompanyId && 
                               t.BranchId == template.BranchId && 
                               t.DocumentTypeId == template.DocumentTypeId &&
                               t.TemplateId != templateId)
                    .CountAsync();

                if (otherTemplates == 0)
                {
                    MessageBox.Show("لا يمكن حذف آخر قالب لنوع المستند هذا",
                        "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                _context.PrintTemplates.Remove(template);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حذف القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<PrintTemplate?> GetDefaultTemplateAsync(int companyId, int? branchId, int documentTypeId, string? locale = null)
        {
            try
            {
                var query = _context.PrintTemplates
                    .Where(t => t.CompanyId == companyId && 
                               t.DocumentTypeId == documentTypeId &&
                               t.Active && t.IsDefault);

                if (branchId.HasValue)
                    query = query.Where(t => t.BranchId == branchId || t.BranchId == null);

                if (!string.IsNullOrEmpty(locale))
                    query = query.Where(t => t.Locale == locale || t.Locale == null);

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع القالب الافتراضي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}