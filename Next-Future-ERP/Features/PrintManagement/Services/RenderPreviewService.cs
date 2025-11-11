using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using Stubble.Core.Builders;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة معاينة القوالب - عرض معاينة للقوالب قبل الطباعة (بدون بيانات تجريبية)
    /// </summary>
    public class RenderPreviewService : IRenderPreviewService
    {
        private readonly AppDbContext _context;

        // مفسّر Mustache (يخدم Liquid البسيط)
        private readonly Stubble.Core.StubbleVisitorRenderer _mustache =
            new StubbleBuilder()
                .Configure(s => s.SetIgnoreCaseOnKeyLookup(true))
                .Build();

        private readonly IDataSourceExecutor _dataSourceExecutor;

        // ✅ لا تعتمد على بيانات تجريبية أبداً (خليه false)
        private const bool USE_SAMPLE_FALLBACK = false;

        public RenderPreviewService(AppDbContext context, IDataSourceExecutor dataSourceExecutor)
        {
            _context = context;
            _dataSourceExecutor = dataSourceExecutor;
        }

        /// <summary>
        /// ينشئ معاينة لإصدار القالب. إذا لم تُمرّر بيانات، سيُرفض الطلب برسالة واضحة.
        /// استخدم RenderPreviewWithDocumentAsync للمعاينة ببيانات مستند فعلية.
        /// </summary>
        public async Task<PreviewResult> RenderPreviewAsync(int templateVersionId, Dictionary<string, object>? sampleData = null)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // تحميل إصدار القالب مع المحتوى
                var templateVersion = await _context.TemplateVersions
                    .Include(tv => tv.Template)
                    .Include(tv => tv.Contents)
                    .FirstOrDefaultAsync(tv => tv.TemplateVersionId == templateVersionId);

                if (templateVersion == null)
                {
                    stopwatch.Stop();
                    return new PreviewResult
                    {
                        Success = false,
                        ErrorMessage = "إصدار القالب غير موجود"
                    };
                }

                // 🚫 لا نولّد بيانات تجريبية تلقائيًا
                if (sampleData == null)
                {
                    if (USE_SAMPLE_FALLBACK)
                    {
                        // لو احتجتها لاحقًا للتجارب غيّر العلم لأجل التطوير فقط
                        sampleData = await GetSampleDataAsync(templateVersion.Template.DocumentTypeId);
                    }
                    else
                    {
                        stopwatch.Stop();
                        return new PreviewResult
                        {
                            Success = false,
                            ErrorMessage = "لا توجد بيانات لعرض المعاينة. مرّر sampleData أو استخدم RenderPreviewWithDocumentAsync(templateVersionId, documentTypeId, documentId).",
                            RenderTime = stopwatch.Elapsed
                        };
                    }
                }

                // إنشاء المعاينة حسب نوع المحرك
                var result = await RenderByEngine(templateVersion, sampleData);

                stopwatch.Stop();
                result.RenderTime = stopwatch.Elapsed;
                result.UsedData = sampleData;

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new PreviewResult
                {
                    Success = false,
                    ErrorMessage = $"خطأ في إنشاء المعاينة: {ex.Message}",
                    RenderTime = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// اختصار مريح يستدعي المعاينة ببيانات المستند الفعلية.
        /// </summary>
        public Task<PreviewResult> RenderPreviewAsync(int templateVersionId, int documentTypeId, long documentId)
            => RenderPreviewWithDocumentAsync(templateVersionId, documentTypeId, documentId);

        private Task<string> ProcessLiquidTemplate(string template, Dictionary<string, object> data)
        {
            var rendered = _mustache.Render(template, data);
            return Task.FromResult(rendered);
        }

        public async Task<PreviewResult> RenderPreviewWithDocumentAsync(int templateVersionId, int documentTypeId, long documentId)
        {
            try
            {
                // الحصول على بيانات المستند الفعلية
                var documentData = await GetDocumentDataAsync(documentTypeId, documentId);

                // مرّر البيانات الحقيقية مباشرةً (بدون أي بيانات تجريبية)
                return await RenderPreviewAsync(templateVersionId, documentData);
            }
            catch (Exception ex)
            {
                return new PreviewResult
                {
                    Success = false,
                    ErrorMessage = $"خطأ في إنشاء المعاينة مع المستند: {ex.Message}"
                };
            }
        }

        public async Task<string> RenderHtmlPreviewAsync(int templateVersionId, Dictionary<string, object>? sampleData = null)
        {
            var result = await RenderPreviewAsync(templateVersionId, sampleData);
            return result.HtmlContent ?? string.Empty;
        }

        public async Task<byte[]> RenderPdfPreviewAsync(int templateVersionId, Dictionary<string, object>? sampleData = null)
        {
            var result = await RenderPreviewAsync(templateVersionId, sampleData);
            return result.PdfContent ?? Array.Empty<byte>();
        }

        /// <summary>
        /// ⚠️ تبقى لأغراض التطوير فقط، لكنها غير مستخدمة افتراضيًا.
        /// </summary>
        public async Task<Dictionary<string, object>> GetSampleDataAsync(int documentTypeId)
        {
            try
            {
                var documentType = await _context.DocumentTypes
                    .FirstOrDefaultAsync(dt => dt.DocumentTypeId == documentTypeId);

                var documentTypeName = documentType?.DocumentNameAr ?? "مستند";

                return documentTypeId switch
                {
                    6 => GetReceiptVoucherSampleData(documentTypeName), // RV
                    7 => GetPaymentVoucherSampleData(documentTypeName), // PV
                    8 => GetInvoiceSampleData(documentTypeName),
                    9 => GetInvoiceSampleData(documentTypeName),
                    1 => GetGenericSampleData(documentTypeName),        // JV placeholder
                    _ => GetGenericSampleData(documentTypeName)
                };
            }
            catch
            {
                return GetGenericSampleData("مستند");
            }
        }

        public async Task<ValidationResult> ValidateTemplateAsync(int templateVersionId)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                var templateVersion = await _context.TemplateVersions
                    .Include(tv => tv.Template)
                    .Include(tv => tv.Contents)
                    .FirstOrDefaultAsync(tv => tv.TemplateVersionId == templateVersionId);

                if (templateVersion == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("إصدار القالب غير موجود");
                    return result;
                }

                if (!templateVersion.Contents.Any())
                {
                    result.IsValid = false;
                    result.Errors.Add("لا يوجد محتوى للقالب");
                }

                if (string.IsNullOrEmpty(templateVersion.Template.Engine))
                {
                    result.IsValid = false;
                    result.Errors.Add("نوع المحرك غير محدد");
                }

                foreach (var content in templateVersion.Contents)
                {
                    if (string.IsNullOrEmpty(content.ContentText) && content.ContentBinary == null)
                    {
                        result.Warnings.Add($"المحتوى من نوع {content.ContentType} فارغ");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"خطأ في التحقق من صحة القالب: {ex.Message}");
                return result;
            }
        }

        private async Task<PreviewResult> RenderByEngine(TemplateVersion templateVersion, Dictionary<string, object> data)
        {
            var engine = templateVersion.Template.Engine?.ToLower();

            return engine switch
            {
                "liquid" => await RenderLiquidTemplate(templateVersion, data),
                "razor" => await RenderRazorTemplate(templateVersion, data),
                "handlebars" => await RenderHandlebarsTemplate(templateVersion, data),
                "freemarker" => await RenderFreeMarkerTemplate(templateVersion, data),
                _ => await RenderSimpleTemplate(templateVersion, data)
            };
        }

        private async Task<PreviewResult> RenderLiquidTemplate(TemplateVersion templateVersion, Dictionary<string, object> data)
        {
            try
            {
                var content =
                    templateVersion.Contents.FirstOrDefault(c => c.ContentType.Equals("liquid", StringComparison.OrdinalIgnoreCase))
                    ?? templateVersion.Contents.FirstOrDefault(c => c.ContentType.Equals("html", StringComparison.OrdinalIgnoreCase))
                    ?? templateVersion.Contents.FirstOrDefault();

                if (content?.ContentText == null)
                {
                    return new PreviewResult
                    {
                        Success = false,
                        ErrorMessage = "لا يوجد محتوى Liquid للقالب"
                    };
                }

                var html = await ProcessLiquidTemplate(content.ContentText, data);

                return new PreviewResult
                {
                    Success = true,
                    HtmlContent = html
                };
            }
            catch (Exception ex)
            {
                return new PreviewResult
                {
                    Success = false,
                    ErrorMessage = $"خطأ في معالجة قالب Liquid: {ex.Message}"
                };
            }
        }

        private Task<PreviewResult> RenderRazorTemplate(TemplateVersion templateVersion, Dictionary<string, object> data)
            => RenderSimpleTemplate(templateVersion, data);

        private Task<PreviewResult> RenderHandlebarsTemplate(TemplateVersion templateVersion, Dictionary<string, object> data)
            => RenderSimpleTemplate(templateVersion, data);

        private Task<PreviewResult> RenderFreeMarkerTemplate(TemplateVersion templateVersion, Dictionary<string, object> data)
            => RenderSimpleTemplate(templateVersion, data);

        private async Task<PreviewResult> RenderSimpleTemplate(TemplateVersion templateVersion, Dictionary<string, object> data)
        {
            try
            {
                var content = templateVersion.Contents.FirstOrDefault();
                if (content?.ContentText == null)
                {
                    return new PreviewResult
                    {
                        Success = false,
                        ErrorMessage = "لا يوجد محتوى للقالب"
                    };
                }

                var html = await ProcessSimpleTemplate(content.ContentText, data);

                return new PreviewResult
                {
                    Success = true,
                    HtmlContent = html
                };
            }
            catch (Exception ex)
            {
                return new PreviewResult
                {
                    Success = false,
                    ErrorMessage = $"خطأ في معالجة القالب: {ex.Message}"
                };
            }
        }

        private Task<string> ProcessSimpleTemplate(string template, Dictionary<string, object> data)
        {
            // استبدال بسيط لنمط {Key} (لو محتوى HTML عادي بدون {{ }})
            var result = template;
            foreach (var item in data)
            {
                var placeholder = $"{{{item.Key}}}";
                var value = item.Value?.ToString() ?? "";
                result = result.Replace(placeholder, value);
            }
            return Task.FromResult(result);
        }

        /// <summary>
        /// يحصل على بيانات المستند الحقيقية عبر الـ DataSourceExecutor.
        /// يعتمد على الإصدار النشط لنوع المستند.
        /// </summary>
        private async Task<Dictionary<string, object>> GetDocumentDataAsync(int documentTypeId, long documentId)
        {
            var version = await _context.TemplateVersions
                .Include(v => v.Template)
                .FirstAsync(v => v.Template.DocumentTypeId == documentTypeId
                                 && v.Status == "active");

            var companyId = version.Template.CompanyId;
            int? branchId = version.Template.BranchId;
            string? locale = version.Template.Locale;

            return await _dataSourceExecutor.GetDocumentDataAsync(
                version.TemplateVersionId, documentTypeId, documentId, companyId, branchId, locale);
        }

        // ===== بيانات تجريبية متروكة لأغراض التطوير فقط (غير مستخدمة افتراضياً) =====

        private Dictionary<string, object> GetReceiptVoucherSampleData(string documentTypeName) => new()
        {
            ["DocumentType"] = documentTypeName,
            ["DocumentNumber"] = "RC-2025-001",
            ["DocumentDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["Amount"] = "1,500.00",
            ["Currency"] = "ريال سعودي",
            ["Description"] = "استلام مبلغ من العميل",
            ["CompanyName"] = "شركة المستقبل التالي",
            ["CompanyAddress"] = "الرياض، المملكة العربية السعودية",
            ["CompanyPhone"] = "+966 11 123 4567",
            ["CompanyEmail"] = "info@nextfuture.com"
        };

        private Dictionary<string, object> GetPaymentVoucherSampleData(string documentTypeName) => new()
        {
            ["DocumentType"] = documentTypeName,
            ["DocumentNumber"] = "PV-2025-001",
            ["DocumentDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["Amount"] = "2,300.00",
            ["Currency"] = "ريال سعودي",
            ["Description"] = "دفع مبلغ للمورد",
            ["CompanyName"] = "شركة المستقبل التالي",
            ["CompanyAddress"] = "الرياض، المملكة العربية السعودية",
            ["CompanyPhone"] = "+966 11 123 4567",
            ["CompanyEmail"] = "info@nextfuture.com"
        };

        private Dictionary<string, object> GetInvoiceSampleData(string documentTypeName) => new()
        {
            ["DocumentType"] = documentTypeName,
            ["InvoiceNumber"] = "INV-2025-001",
            ["InvoiceDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["DueDate"] = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
            ["SubTotal"] = "1,000.00",
            ["VATAmount"] = "150.00",
            ["TotalAmount"] = "1,150.00",
            ["Currency"] = "ريال سعودي",
            ["CustomerName"] = "العميل التجريبي",
            ["CustomerAddress"] = "عنوان العميل",
            ["CompanyName"] = "شركة المستقبل التالي",
            ["CompanyAddress"] = "الرياض، المملكة العربية السعودية",
            ["CompanyPhone"] = "+966 11 123 4567",
            ["CompanyEmail"] = "info@nextfuture.com"
        };

        private Dictionary<string, object> GetPurchaseOrderSampleData(string documentTypeName) => new()
        {
            ["DocumentType"] = documentTypeName,
            ["OrderNumber"] = "PO-2025-001",
            ["OrderDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["ExpectedDelivery"] = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
            ["SubTotal"] = "5,000.00",
            ["VATAmount"] = "750.00",
            ["TotalAmount"] = "5,750.00",
            ["Currency"] = "ريال سعودي",
            ["SupplierName"] = "المورد التجريبي",
            ["SupplierAddress"] = "عنوان المورد",
            ["CompanyName"] = "شركة المستقبل التالي",
            ["CompanyAddress"] = "الرياض، المملكة العربية السعودية",
            ["CompanyPhone"] = "+966 11 123 4567",
            ["CompanyEmail"] = "info@nextfuture.com"
        };

        private Dictionary<string, object> GetGenericSampleData(string documentTypeName) => new()
        {
            ["DocumentType"] = documentTypeName,
            ["DocumentNumber"] = "DOC-2025-001",
            ["DocumentDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["Amount"] = "1,000.00",
            ["Currency"] = "ريال سعودي",
            ["Description"] = "مستند تجريبي",
            ["CompanyName"] = "شركة المستقبل التالي",
            ["CompanyAddress"] = "الرياض، المملكة العربية السعودية",
            ["CompanyPhone"] = "+966 11 123 4567",
            ["CompanyEmail"] = "info@nextfuture.com"
        };
    }
}
