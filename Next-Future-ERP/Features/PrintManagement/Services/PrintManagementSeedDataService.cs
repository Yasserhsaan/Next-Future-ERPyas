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
    /// خدمة إضافة البيانات التجريبية لنظام إدارة الطباعة
    /// </summary>
    public class PrintManagementSeedDataService
    {
        private readonly AppDbContext _context;

        public PrintManagementSeedDataService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// إضافة البيانات التجريبية لنظام إدارة الطباعة
        /// </summary>
        public async Task SeedPrintManagementDataAsync()
        {
            try
            {
                // التحقق من وجود البيانات مسبقاً
                if (await _context.PrintTemplates.AnyAsync())
                {
                    return; // البيانات موجودة بالفعل
                }

                // إضافة قوالب الطباعة التجريبية
                await SeedPrintTemplatesAsync();
                
                // إضافة أصول الطباعة التجريبية
                await SeedPrintAssetsAsync();

                MessageBox.Show("✅ تم إضافة البيانات التجريبية لنظام إدارة الطباعة بنجاح",
                    "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في إضافة البيانات التجريبية:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SeedPrintTemplatesAsync()
        {
            var templates = new List<PrintTemplate>
            {
                // قالب سند قبض
                new PrintTemplate
                {
                    CompanyId = 1,
                    BranchId = 1,
                    DocumentTypeId = 1, // سند قبض
                    Name = "قالب سند قبض افتراضي",
                    Engine = "Liquid",
                    PaperSize = "A4",
                    Orientation = "P",
                    Locale = "ar-SA",
                    IsDefault = true,
                    Active = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                // قالب سند دفع
                new PrintTemplate
                {
                    CompanyId = 1,
                    BranchId = 1,
                    DocumentTypeId = 2, // سند دفع
                    Name = "قالب سند دفع افتراضي",
                    Engine = "Liquid",
                    PaperSize = "A4",
                    Orientation = "P",
                    Locale = "ar-SA",
                    IsDefault = true,
                    Active = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                // قالب فاتورة
                new PrintTemplate
                {
                    CompanyId = 1,
                    BranchId = 1,
                    DocumentTypeId = 3, // فاتورة
                    Name = "قالب فاتورة افتراضي",
                    Engine = "Liquid",
                    PaperSize = "A4",
                    Orientation = "P",
                    Locale = "ar-SA",
                    IsDefault = true,
                    Active = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                },
                // قالب أمر شراء
                new PrintTemplate
                {
                    CompanyId = 1,
                    BranchId = 1,
                    DocumentTypeId = 4, // أمر شراء
                    Name = "قالب أمر شراء افتراضي",
                    Engine = "Liquid",
                    PaperSize = "A4",
                    Orientation = "P",
                    Locale = "ar-SA",
                    IsDefault = true,
                    Active = true,
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.PrintTemplates.AddRange(templates);
            await _context.SaveChangesAsync();

            // إضافة إصدارات القوالب
            await SeedTemplateVersionsAsync(templates);
        }

        private async Task SeedTemplateVersionsAsync(List<PrintTemplate> templates)
        {
            var templateVersions = new List<TemplateVersion>();

            foreach (var template in templates)
            {
                var version = new TemplateVersion
                {
                    TemplateId = template.TemplateId,
                    VersionNo = 1,
                    Status = "active",
                    Notes = "الإصدار الأول من القالب",
                    CreatedBy = 1,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                };

                templateVersions.Add(version);
            }

            _context.TemplateVersions.AddRange(templateVersions);
            await _context.SaveChangesAsync();

            // إضافة محتوى القوالب
            await SeedTemplateContentsAsync(templateVersions);
        }

        private async Task SeedTemplateContentsAsync(List<TemplateVersion> templateVersions)
        {
            var templateContents = new List<TemplateContent>();

            foreach (var version in templateVersions)
            {
                // محتوى HTML
                var htmlContent = new TemplateContent
                {
                    TemplateVersionId = version.TemplateVersionId,
                    ContentType = "html",
                    ContentText = GetHtmlTemplate(version.TemplateId),
                    ContentHash = new byte[32] // يمكن حساب hash حقيقي لاحقاً
                };

                // محتوى Liquid
                var liquidContent = new TemplateContent
                {
                    TemplateVersionId = version.TemplateVersionId,
                    ContentType = "liquid",
                    ContentText = GetLiquidTemplate(version.TemplateId),
                    ContentHash = new byte[32]
                };

                templateContents.Add(htmlContent);
                templateContents.Add(liquidContent);
            }

            foreach (var content in templateContents)
            {
                _context.TemplateContents.Add(content);
            }
            await _context.SaveChangesAsync();

            // إضافة مصادر البيانات
            await SeedTemplateDataSourcesAsync(templateVersions);
        }

        private async Task SeedTemplateDataSourcesAsync(List<TemplateVersion> templateVersions)
        {
            var dataSources = new List<TemplateDataSource>();

            foreach (var version in templateVersions)
            {
                var dataSource = new TemplateDataSource
                {
                    TemplateVersionId = version.TemplateVersionId,
                    Name = "MainDataSource",
                    SourceType = "sql",
                    SourceName = "GetDocumentData",
                    IsMain = true,
                    TimeoutSec = 30
                };

                dataSources.Add(dataSource);
            }

            _context.TemplateDataSources.AddRange(dataSources);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPrintAssetsAsync()
        {
            var assets = new List<PrintAsset>
            {
                new PrintAsset
                {
                    CompanyId = 1,
                    BranchId = 1,
                    Name = "شعار الشركة",
                    MimeType = "image/png",
                    Url = "/assets/company-logo.png",
                    CreatedAt = DateTime.UtcNow
                },
                new PrintAsset
                {
                    CompanyId = 1,
                    BranchId = 1,
                    Name = "خلفية الفاتورة",
                    MimeType = "image/jpeg",
                    Url = "/assets/invoice-background.jpg",
                    CreatedAt = DateTime.UtcNow
                },
                new PrintAsset
                {
                    CompanyId = 1,
                    BranchId = 1,
                    Name = "خط عربي",
                    MimeType = "font/ttf",
                    Url = "/assets/arabic-font.ttf",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.PrintAssets.AddRange(assets);
            await _context.SaveChangesAsync();
        }

        private string GetHtmlTemplate(int templateId)
        {
            return templateId switch
            {
                1 => GetReceiptVoucherHtmlTemplate(),
                2 => GetPaymentVoucherHtmlTemplate(),
                3 => GetInvoiceHtmlTemplate(),
                4 => GetPurchaseOrderHtmlTemplate(),
                _ => GetGenericHtmlTemplate()
            };
        }

        private string GetLiquidTemplate(int templateId)
        {
            return templateId switch
            {
                1 => GetReceiptVoucherLiquidTemplate(),
                2 => GetPaymentVoucherLiquidTemplate(),
                3 => GetInvoiceLiquidTemplate(),
                4 => GetPurchaseOrderLiquidTemplate(),
                _ => GetGenericLiquidTemplate()
            };
        }

        private string GetReceiptVoucherHtmlTemplate()
        {
            return @"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <title>سند قبض</title>
    <style>
        body { font-family: 'Arial', sans-serif; margin: 20px; }
        .header { text-align: center; margin-bottom: 30px; }
        .company-name { font-size: 24px; font-weight: bold; }
        .document-title { font-size: 20px; margin: 10px 0; }
        .document-info { margin: 20px 0; }
        .info-row { display: flex; justify-content: space-between; margin: 10px 0; }
        .amount { font-size: 18px; font-weight: bold; color: #2c5aa0; }
        .footer { margin-top: 50px; text-align: center; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{{ CompanyName }}</div>
        <div class='document-title'>سند قبض</div>
    </div>
    
    <div class='document-info'>
        <div class='info-row'>
            <span>رقم السند:</span>
            <span>{{ DocumentNumber }}</span>
        </div>
        <div class='info-row'>
            <span>التاريخ:</span>
            <span>{{ DocumentDate }}</span>
        </div>
        <div class='info-row'>
            <span>المبلغ:</span>
            <span class='amount'>{{ Amount }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>الوصف:</span>
            <span>{{ Description }}</span>
        </div>
    </div>
    
    <div class='footer'>
        <p>{{ CompanyAddress }}</p>
        <p>هاتف: {{ CompanyPhone }} | بريد إلكتروني: {{ CompanyEmail }}</p>
    </div>
</body>
</html>";
        }

        private string GetPaymentVoucherHtmlTemplate()
        {
            return @"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <title>سند دفع</title>
    <style>
        body { font-family: 'Arial', sans-serif; margin: 20px; }
        .header { text-align: center; margin-bottom: 30px; }
        .company-name { font-size: 24px; font-weight: bold; }
        .document-title { font-size: 20px; margin: 10px 0; }
        .document-info { margin: 20px 0; }
        .info-row { display: flex; justify-content: space-between; margin: 10px 0; }
        .amount { font-size: 18px; font-weight: bold; color: #d32f2f; }
        .footer { margin-top: 50px; text-align: center; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{{ CompanyName }}</div>
        <div class='document-title'>سند دفع</div>
    </div>
    
    <div class='document-info'>
        <div class='info-row'>
            <span>رقم السند:</span>
            <span>{{ DocumentNumber }}</span>
        </div>
        <div class='info-row'>
            <span>التاريخ:</span>
            <span>{{ DocumentDate }}</span>
        </div>
        <div class='info-row'>
            <span>المبلغ:</span>
            <span class='amount'>{{ Amount }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>الوصف:</span>
            <span>{{ Description }}</span>
        </div>
    </div>
    
    <div class='footer'>
        <p>{{ CompanyAddress }}</p>
        <p>هاتف: {{ CompanyPhone }} | بريد إلكتروني: {{ CompanyEmail }}</p>
    </div>
</body>
</html>";
        }

        private string GetInvoiceHtmlTemplate()
        {
            return @"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <title>فاتورة</title>
    <style>
        body { font-family: 'Arial', sans-serif; margin: 20px; }
        .header { text-align: center; margin-bottom: 30px; }
        .company-name { font-size: 24px; font-weight: bold; }
        .document-title { font-size: 20px; margin: 10px 0; }
        .document-info { margin: 20px 0; }
        .info-row { display: flex; justify-content: space-between; margin: 10px 0; }
        .amount { font-size: 18px; font-weight: bold; color: #2c5aa0; }
        .footer { margin-top: 50px; text-align: center; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{{ CompanyName }}</div>
        <div class='document-title'>فاتورة</div>
    </div>
    
    <div class='document-info'>
        <div class='info-row'>
            <span>رقم الفاتورة:</span>
            <span>{{ InvoiceNumber }}</span>
        </div>
        <div class='info-row'>
            <span>تاريخ الفاتورة:</span>
            <span>{{ InvoiceDate }}</span>
        </div>
        <div class='info-row'>
            <span>تاريخ الاستحقاق:</span>
            <span>{{ DueDate }}</span>
        </div>
        <div class='info-row'>
            <span>المجموع الفرعي:</span>
            <span>{{ SubTotal }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>ضريبة القيمة المضافة:</span>
            <span>{{ VATAmount }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>المجموع الكلي:</span>
            <span class='amount'>{{ TotalAmount }} {{ Currency }}</span>
        </div>
    </div>
    
    <div class='footer'>
        <p>{{ CompanyAddress }}</p>
        <p>هاتف: {{ CompanyPhone }} | بريد إلكتروني: {{ CompanyEmail }}</p>
    </div>
</body>
</html>";
        }

        private string GetPurchaseOrderHtmlTemplate()
        {
            return @"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <title>أمر شراء</title>
    <style>
        body { font-family: 'Arial', sans-serif; margin: 20px; }
        .header { text-align: center; margin-bottom: 30px; }
        .company-name { font-size: 24px; font-weight: bold; }
        .document-title { font-size: 20px; margin: 10px 0; }
        .document-info { margin: 20px 0; }
        .info-row { display: flex; justify-content: space-between; margin: 10px 0; }
        .amount { font-size: 18px; font-weight: bold; color: #2c5aa0; }
        .footer { margin-top: 50px; text-align: center; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{{ CompanyName }}</div>
        <div class='document-title'>أمر شراء</div>
    </div>
    
    <div class='document-info'>
        <div class='info-row'>
            <span>رقم الأمر:</span>
            <span>{{ OrderNumber }}</span>
        </div>
        <div class='info-row'>
            <span>تاريخ الأمر:</span>
            <span>{{ OrderDate }}</span>
        </div>
        <div class='info-row'>
            <span>تاريخ التسليم المتوقع:</span>
            <span>{{ ExpectedDelivery }}</span>
        </div>
        <div class='info-row'>
            <span>المجموع الفرعي:</span>
            <span>{{ SubTotal }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>ضريبة القيمة المضافة:</span>
            <span>{{ VATAmount }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>المجموع الكلي:</span>
            <span class='amount'>{{ TotalAmount }} {{ Currency }}</span>
        </div>
    </div>
    
    <div class='footer'>
        <p>{{ CompanyAddress }}</p>
        <p>هاتف: {{ CompanyPhone }} | بريد إلكتروني: {{ CompanyEmail }}</p>
    </div>
</body>
</html>";
        }

        private string GetGenericHtmlTemplate()
        {
            return @"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <title>{{ DocumentType }}</title>
    <style>
        body { font-family: 'Arial', sans-serif; margin: 20px; }
        .header { text-align: center; margin-bottom: 30px; }
        .company-name { font-size: 24px; font-weight: bold; }
        .document-title { font-size: 20px; margin: 10px 0; }
        .document-info { margin: 20px 0; }
        .info-row { display: flex; justify-content: space-between; margin: 10px 0; }
        .amount { font-size: 18px; font-weight: bold; color: #2c5aa0; }
        .footer { margin-top: 50px; text-align: center; }
    </style>
</head>
<body>
    <div class='header'>
        <div class='company-name'>{{ CompanyName }}</div>
        <div class='document-title'>{{ DocumentType }}</div>
    </div>
    
    <div class='document-info'>
        <div class='info-row'>
            <span>رقم المستند:</span>
            <span>{{ DocumentNumber }}</span>
        </div>
        <div class='info-row'>
            <span>التاريخ:</span>
            <span>{{ DocumentDate }}</span>
        </div>
        <div class='info-row'>
            <span>المبلغ:</span>
            <span class='amount'>{{ Amount }} {{ Currency }}</span>
        </div>
        <div class='info-row'>
            <span>الوصف:</span>
            <span>{{ Description }}</span>
        </div>
    </div>
    
    <div class='footer'>
        <p>{{ CompanyAddress }}</p>
        <p>هاتف: {{ CompanyPhone }} | بريد إلكتروني: {{ CompanyEmail }}</p>
    </div>
</body>
</html>";
        }

        private string GetReceiptVoucherLiquidTemplate()
        {
            return @"
<div class='header'>
    <div class='company-name'>{{ CompanyName }}</div>
    <div class='document-title'>سند قبض</div>
</div>

<div class='document-info'>
    <div class='info-row'>
        <span>رقم السند:</span>
        <span>{{ DocumentNumber }}</span>
    </div>
    <div class='info-row'>
        <span>التاريخ:</span>
        <span>{{ DocumentDate }}</span>
    </div>
    <div class='info-row'>
        <span>المبلغ:</span>
        <span class='amount'>{{ Amount }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>الوصف:</span>
        <span>{{ Description }}</span>
    </div>
</div>";
        }

        private string GetPaymentVoucherLiquidTemplate()
        {
            return @"
<div class='header'>
    <div class='company-name'>{{ CompanyName }}</div>
    <div class='document-title'>سند دفع</div>
</div>

<div class='document-info'>
    <div class='info-row'>
        <span>رقم السند:</span>
        <span>{{ DocumentNumber }}</span>
    </div>
    <div class='info-row'>
        <span>التاريخ:</span>
        <span>{{ DocumentDate }}</span>
    </div>
    <div class='info-row'>
        <span>المبلغ:</span>
        <span class='amount'>{{ Amount }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>الوصف:</span>
        <span>{{ Description }}</span>
    </div>
</div>";
        }

        private string GetInvoiceLiquidTemplate()
        {
            return @"
<div class='header'>
    <div class='company-name'>{{ CompanyName }}</div>
    <div class='document-title'>فاتورة</div>
</div>

<div class='document-info'>
    <div class='info-row'>
        <span>رقم الفاتورة:</span>
        <span>{{ InvoiceNumber }}</span>
    </div>
    <div class='info-row'>
        <span>تاريخ الفاتورة:</span>
        <span>{{ InvoiceDate }}</span>
    </div>
    <div class='info-row'>
        <span>تاريخ الاستحقاق:</span>
        <span>{{ DueDate }}</span>
    </div>
    <div class='info-row'>
        <span>المجموع الفرعي:</span>
        <span>{{ SubTotal }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>ضريبة القيمة المضافة:</span>
        <span>{{ VATAmount }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>المجموع الكلي:</span>
        <span class='amount'>{{ TotalAmount }} {{ Currency }}</span>
    </div>
</div>";
        }

        private string GetPurchaseOrderLiquidTemplate()
        {
            return @"
<div class='header'>
    <div class='company-name'>{{ CompanyName }}</div>
    <div class='document-title'>أمر شراء</div>
</div>

<div class='document-info'>
    <div class='info-row'>
        <span>رقم الأمر:</span>
        <span>{{ OrderNumber }}</span>
    </div>
    <div class='info-row'>
        <span>تاريخ الأمر:</span>
        <span>{{ OrderDate }}</span>
    </div>
    <div class='info-row'>
        <span>تاريخ التسليم المتوقع:</span>
        <span>{{ ExpectedDelivery }}</span>
    </div>
    <div class='info-row'>
        <span>المجموع الفرعي:</span>
        <span>{{ SubTotal }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>ضريبة القيمة المضافة:</span>
        <span>{{ VATAmount }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>المجموع الكلي:</span>
        <span class='amount'>{{ TotalAmount }} {{ Currency }}</span>
    </div>
</div>";
        }

        private string GetGenericLiquidTemplate()
        {
            return @"
<div class='header'>
    <div class='company-name'>{{ CompanyName }}</div>
    <div class='document-title'>{{ DocumentType }}</div>
</div>

<div class='document-info'>
    <div class='info-row'>
        <span>رقم المستند:</span>
        <span>{{ DocumentNumber }}</span>
    </div>
    <div class='info-row'>
        <span>التاريخ:</span>
        <span>{{ DocumentDate }}</span>
    </div>
    <div class='info-row'>
        <span>المبلغ:</span>
        <span class='amount'>{{ Amount }} {{ Currency }}</span>
    </div>
    <div class='info-row'>
        <span>الوصف:</span>
        <span>{{ Description }}</span>
    </div>
</div>";
        }
    }
}
