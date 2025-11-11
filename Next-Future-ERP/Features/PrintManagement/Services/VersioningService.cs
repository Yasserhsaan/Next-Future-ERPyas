using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TemplateContent = Next_Future_ERP.Features.PrintManagement.Models.TemplateContent;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// تنفيذ خدمة إدارة إصدارات القوالب
    /// </summary>
    public class VersioningService : IVersioningService
    {
        private readonly AppDbContext _context;

        public VersioningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VersionInfo>> GetVersionsAsync(int templateId)
        {
            try
            {
                var versions = await _context.TemplateVersions
                    .Where(v => v.TemplateId == templateId)
                    .Include(v => v.Contents)
                    .Include(v => v.DataSources)
                    .OrderByDescending(v => v.VersionNo)
                    .AsNoTracking()
                    .ToListAsync();

                return versions.Select(v => new VersionInfo
                {
                    TemplateVersionId = v.TemplateVersionId,
                    VersionNo = v.VersionNo,
                    Status = v.Status,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    ActivatedAt = v.ActivatedAt,
                    CreatedByName = $"مستخدم {v.CreatedBy}", // يمكن تحسينها من جدول المستخدمين
                    ContentCount = v.Contents.Count,
                    DataSourceCount = v.DataSources.Count,
                    HasMainDataSource = v.DataSources.Any(ds => ds.IsMain)
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع إصدارات القالب:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<VersionInfo>();
            }
        }

        public async Task<TemplateVersion?> GetVersionByIdAsync(int versionId)
        {
            try
            {
                return await _context.TemplateVersions
                    .Include(v => v.Template)
                    .Include(v => v.Contents)
                    .Include(v => v.DataSources)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.TemplateVersionId == versionId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<TemplateVersion?> GetActiveVersionAsync(int templateId)
        {
            try
            {
                return await _context.TemplateVersions
                    .Include(v => v.Contents)
                    .Include(v => v.DataSources)
                    .Where(v => v.TemplateId == templateId && v.Status == TemplateVersionStatus.Active)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع الإصدار النشط:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<TemplateVersion> CreateNewVersionAsync(int templateId, string? notes = null)
        {
            try
            {
                var nextVersionNo = await GetNextVersionNumberAsync(templateId);

                var newVersion = new TemplateVersion
                {
                    TemplateId = templateId,
                    VersionNo = nextVersionNo,
                    Status = TemplateVersionStatus.Draft,
                    Notes = notes ?? $"الإصدار رقم {nextVersionNo}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.TemplateVersions.Add(newVersion);
                await _context.SaveChangesAsync();

                return newVersion;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إنشاء الإصدار الجديد:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<TemplateVersion> CreateVersionFromExistingAsync(int templateId, int sourceVersionId, string? notes = null)
        {
            try
            {
                var sourceVersion = await _context.TemplateVersions
                    .Include(v => v.Contents)
                    .Include(v => v.DataSources)
                    .FirstOrDefaultAsync(v => v.TemplateVersionId == sourceVersionId);

                if (sourceVersion == null)
                    throw new ArgumentException("الإصدار المصدر غير موجود");

                var nextVersionNo = await GetNextVersionNumberAsync(templateId);

                var newVersion = new TemplateVersion
                {
                    TemplateId = templateId,
                    VersionNo = nextVersionNo,
                    Status = TemplateVersionStatus.Draft,
                    Notes = notes ?? $"منسوخ من الإصدار {sourceVersion.VersionNo}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.TemplateVersions.Add(newVersion);
                await _context.SaveChangesAsync();

                // نسخ المحتويات
                foreach (var content in sourceVersion.Contents)
                {
                    var newContent = new TemplateContent
                    {
                        TemplateVersionId = newVersion.TemplateVersionId,
                        ContentType = content.ContentType,
                        ContentText = content.ContentText,
                        ContentBinary = content.ContentBinary,
                        ContentHash = content.ContentHash
                    };
                    _context.TemplateContents.Add(newContent);
                }

                // نسخ مصادر البيانات
                foreach (var dataSource in sourceVersion.DataSources)
                {
                    var newDataSource = new TemplateDataSource
                    {
                        TemplateVersionId = newVersion.TemplateVersionId,
                        Name = dataSource.Name,
                        SourceType = dataSource.SourceType,
                        SourceName = dataSource.SourceName,
                        IsMain = dataSource.IsMain,
                        TimeoutSec = dataSource.TimeoutSec
                    };
                    _context.TemplateDataSources.Add(newDataSource);
                }

                await _context.SaveChangesAsync();
                return newVersion;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في نسخ الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> ActivateVersionAsync(int versionId, string? notes = null)
        {
            try
            {
                var version = await _context.TemplateVersions
                    .Include(v => v.Template)
                    .FirstOrDefaultAsync(v => v.TemplateVersionId == versionId);

                if (version == null) return false;

                // التحقق من صحة الإصدار
                var (isValid, errors) = await ValidateVersionForActivationAsync(versionId);
                if (!isValid)
                {
                    var errorMessage = string.Join("\n", errors);
                    MessageBox.Show($"لا يمكن تفعيل الإصدار:\n{errorMessage}",
                        "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // أرشفة الإصدار النشط الحالي
                var currentActive = await _context.TemplateVersions
                    .Where(v => v.TemplateId == version.TemplateId && v.Status == TemplateVersionStatus.Active)
                    .FirstOrDefaultAsync();

                if (currentActive != null)
                {
                    currentActive.Status = TemplateVersionStatus.Archived;
                }

                // تفعيل الإصدار الجديد
                version.Status = TemplateVersionStatus.Active;
                version.ActivatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(notes))
                {
                    version.Notes = notes;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تفعيل الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> ArchiveVersionAsync(int versionId, string? notes = null)
        {
            try
            {
                var version = await _context.TemplateVersions.FindAsync(versionId);
                if (version == null) return false;

                if (version.Status == TemplateVersionStatus.Active)
                {
                    MessageBox.Show("لا يمكن أرشفة الإصدار النشط مباشرة، يجب تفعيل إصدار آخر أولاً",
                        "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                version.Status = TemplateVersionStatus.Archived;
                if (!string.IsNullOrEmpty(notes))
                {
                    version.Notes = notes;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في أرشفة الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RevertToVersionAsync(int versionId, string? notes = null)
        {
            try
            {
                return await ActivateVersionAsync(versionId, notes ?? "استرجاع إلى إصدار سابق");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في الاسترجاع:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateVersionNotesAsync(int versionId, string notes)
        {
            try
            {
                var version = await _context.TemplateVersions.FindAsync(versionId);
                if (version == null) return false;

                version.Notes = notes;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحديث ملاحظات الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteVersionAsync(int versionId)
        {
            try
            {
                var version = await _context.TemplateVersions
                    .Include(v => v.PrintJobs)
                    .FirstOrDefaultAsync(v => v.TemplateVersionId == versionId);

                if (version == null) return false;

                if (version.Status != TemplateVersionStatus.Draft)
                {
                    MessageBox.Show("لا يمكن حذف إصدار غير مسودة",
                        "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (version.PrintJobs.Any())
                {
                    MessageBox.Show("لا يمكن حذف الإصدار لأنه يحتوي على مهام طباعة",
                        "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                _context.TemplateVersions.Remove(version);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حذف الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<(bool IsValid, List<string> ValidationErrors)> ValidateVersionForActivationAsync(int versionId)
        {
            try
            {
                var errors = new List<string>();

                var version = await _context.TemplateVersions
                    .Include(v => v.Contents)
                    .Include(v => v.DataSources)
                    .FirstOrDefaultAsync(v => v.TemplateVersionId == versionId);

                if (version == null)
                {
                    errors.Add("الإصدار غير موجود");
                    return (false, errors);
                }

                // التحقق من وجود محتوى أساسي
                var hasBasicContent = version.Contents.Any(c => 
                    c.ContentType == TemplateContentType.Html || 
                    c.ContentType == TemplateContentType.Jrxml || 
                    c.ContentType == TemplateContentType.Fr3);

                if (!hasBasicContent)
                {
                    errors.Add("يجب أن يحتوي الإصدار على محتوى أساسي (HTML أو JRXML أو FR3)");
                }

                // التحقق من وجود مصدر بيانات رئيسي
                var hasMainDataSource = version.DataSources.Any(ds => ds.IsMain);
                if (!hasMainDataSource)
                {
                    errors.Add("يجب أن يحتوي الإصدار على مصدر بيانات رئيسي واحد على الأقل");
                }

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التحقق من صحة الإصدار:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return (false, new List<string> { "خطأ في التحقق من صحة الإصدار" });
            }
        }

        public async Task<int> GetNextVersionNumberAsync(int templateId)
        {
            try
            {
                var maxVersion = await _context.TemplateVersions
                    .Where(v => v.TemplateId == templateId)
                    .MaxAsync(v => (int?)v.VersionNo) ?? 0;

                return maxVersion + 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في الحصول على رقم الإصدار التالي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
        }
    }
}
