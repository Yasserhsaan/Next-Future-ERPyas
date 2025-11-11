using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TemplateContent = Next_Future_ERP.Features.PrintManagement.Models.TemplateContent;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// تنفيذ خدمة إدارة المحتوى
    /// </summary>
    public class ContentService : IContentService
    {
        private readonly AppDbContext _context;

        public ContentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TemplateContent>> GetVersionContentsAsync(int versionId)
        {
            try
            {
                return await _context.TemplateContents
                    .Where(c => c.TemplateVersionId == versionId)
                    .OrderBy(c => c.ContentType)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في استرجاع المحتويات:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<TemplateContent>();
            }
        }

        public async Task<TemplateContent> AddTextContentAsync(int versionId, string contentType, string content)
        {
            try
            {
                // حذف المحتوى الموجود من نفس النوع
                var existing = await _context.TemplateContents
                    .Where(c => c.TemplateVersionId == versionId && c.ContentType == contentType)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    _context.TemplateContents.Remove(existing);
                }

                // إنشاء محتوى جديد
                var hash = ComputeHash(Encoding.UTF8.GetBytes(content));
                var newContent = new TemplateContent
                {
                    TemplateVersionId = versionId,
                    ContentType = contentType,
                    ContentText = content,
                    ContentHash = hash
                };

                _context.TemplateContents.Add(newContent);
                await _context.SaveChangesAsync();

                return newContent;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إضافة المحتوى النصي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<TemplateContent> AddBinaryContentAsync(int versionId, string contentType, byte[] content)
        {
            try
            {
                // حذف المحتوى الموجود من نفس النوع
                var existing = await _context.TemplateContents
                    .Where(c => c.TemplateVersionId == versionId && c.ContentType == contentType)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    _context.TemplateContents.Remove(existing);
                }

                // إنشاء محتوى جديد
                var hash = ComputeHash(content);
                var newContent = new TemplateContent
                {
                    TemplateVersionId = versionId,
                    ContentType = contentType,
                    ContentBinary = content,
                    ContentHash = hash
                };

                _context.TemplateContents.Add(newContent);
                await _context.SaveChangesAsync();

                return newContent;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إضافة المحتوى الثنائي:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> UpdateContentAsync(int contentId, string? textContent = null, byte[]? binaryContent = null)
        {
            try
            {
                var content = await _context.TemplateContents.FindAsync(contentId);
                if (content == null) return false;

                if (textContent != null)
                {
                    content.ContentText = textContent;
                    content.ContentBinary = null;
                    content.ContentHash = ComputeHash(Encoding.UTF8.GetBytes(textContent));
                }
                else if (binaryContent != null)
                {
                    content.ContentBinary = binaryContent;
                    content.ContentText = null;
                    content.ContentHash = ComputeHash(binaryContent);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحديث المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> RemoveContentAsync(int contentId)
        {
            try
            {
                var content = await _context.TemplateContents.FindAsync(contentId);
                if (content == null) return false;

                _context.TemplateContents.Remove(content);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في حذف المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<(bool IsValid, List<string> ValidationErrors)> ValidateContentAsync(int contentId)
        {
            try
            {
                var errors = new List<string>();
                var content = await _context.TemplateContents.FindAsync(contentId);

                if (content == null)
                {
                    errors.Add("المحتوى غير موجود");
                    return (false, errors);
                }

                // التحقق من وجود محتوى
                var hasContent = !string.IsNullOrEmpty(content.ContentText) || 
                                (content.ContentBinary != null && content.ContentBinary.Length > 0);

                if (!hasContent)
                {
                    errors.Add("المحتوى فارغ");
                }

                // التحقق من صحة HTML
                if (content.ContentType == TemplateContentType.Html && !string.IsNullOrEmpty(content.ContentText))
                {
                    if (!content.ContentText.Contains("<html") && !content.ContentText.Contains("<body"))
                    {
                        errors.Add("محتوى HTML يجب أن يحتوي على عناصر html أو body");
                    }
                }

                // التحقق من صحة CSS
                if (content.ContentType == TemplateContentType.Css && !string.IsNullOrEmpty(content.ContentText))
                {
                    // تحقق بسيط من وجود قواعد CSS
                    if (!content.ContentText.Contains("{") || !content.ContentText.Contains("}"))
                    {
                        errors.Add("محتوى CSS يجب أن يحتوي على قواعد صالحة");
                    }
                }

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في التحقق من صحة المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return (false, new List<string> { "خطأ في التحقق من صحة المحتوى" });
            }
        }

        public async Task<(byte[] Data, string MimeType, string FileName)?> GetContentForDownloadAsync(int contentId)
        {
            try
            {
                var content = await _context.TemplateContents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.TemplateContentId == contentId);

                if (content == null) return null;

                byte[] data;
                string mimeType;
                string fileName;

                if (!string.IsNullOrEmpty(content.ContentText))
                {
                    // محتوى نصي
                    data = Encoding.UTF8.GetBytes(content.ContentText);
                    mimeType = content.ContentType switch
                    {
                        TemplateContentType.Html => "text/html",
                        TemplateContentType.Css => "text/css",
                        _ => "text/plain"
                    };
                    fileName = $"content.{content.ContentType}";
                }
                else if (content.ContentBinary != null)
                {
                    // محتوى ثنائي
                    data = content.ContentBinary;
                    mimeType = content.ContentType switch
                    {
                        TemplateContentType.Jrxml => "application/xml",
                        TemplateContentType.Fr3 => "application/octet-stream",
                        _ => "application/octet-stream"
                    };
                    fileName = $"content.{content.ContentType}";
                }
                else
                {
                    return null;
                }

                return (data, mimeType, fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تحميل المحتوى:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<TemplateContent> UploadContentFileAsync(int versionId, string contentType, Stream fileStream, string fileName)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();

                // تحديد نوع المحتوى بناءً على امتداد الملف
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var actualContentType = extension switch
                {
                    ".html" or ".htm" => TemplateContentType.Html,
                    ".css" => TemplateContentType.Css,
                    ".jrxml" => TemplateContentType.Jrxml,
                    ".fr3" => TemplateContentType.Fr3,
                    _ => contentType
                };

                // للملفات النصية، تحويل إلى نص
                if (actualContentType == TemplateContentType.Html || actualContentType == TemplateContentType.Css)
                {
                    var textContent = Encoding.UTF8.GetString(content);
                    return await AddTextContentAsync(versionId, actualContentType, textContent);
                }
                else
                {
                    return await AddBinaryContentAsync(versionId, actualContentType, content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في رفع الملف:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private static byte[] ComputeHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(data);
        }
    }
}
