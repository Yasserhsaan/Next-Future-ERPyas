-- إدراج بيانات تجريبية لنظام إدارة قوالب الطباعة
-- Sample Data for Print Template Management System

USE [NextFutureERP]
GO

-- إدراج قالب تجريبي لفاتورة المبيعات
INSERT INTO [dbo].[PrintTemplates] (
    [CompanyId], [BranchId], [DocumentTypeId], [Name], [Engine], 
    [PaperSize], [Orientation], [Locale], [IsDefault], [Active], 
    [CreatedBy], [CreatedAt]
) VALUES (
    1, NULL, 1, N'قالب فاتورة المبيعات الافتراضي', 'html',
    'A4', 'P', 'ar-SA', 1, 1,
    1, SYSDATETIME()
);

DECLARE @TemplateId INT = SCOPE_IDENTITY();

-- إدراج الإصدار الأول للقالب
INSERT INTO [dbo].[TemplateVersions] (
    [TemplateId], [VersionNo], [Status], [Notes], 
    [CreatedBy], [CreatedAt], [ActivatedAt]
) VALUES (
    @TemplateId, 1, 'active', N'الإصدار الأولي للقالب',
    1, SYSDATETIME(), SYSDATETIME()
);

DECLARE @VersionId INT = SCOPE_IDENTITY();

-- إدراج محتوى HTML للقالب
INSERT INTO [dbo].[TemplateContents] (
    [TemplateVersionId], [ContentType], [ContentText], [ContentHash]
) VALUES (
    @VersionId, 'html', N'<!DOCTYPE html>
<html dir="rtl" lang="ar">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>فاتورة مبيعات</title>
    <style>
        body { font-family: ''Segoe UI'', Tahoma, Arial, sans-serif; direction: rtl; margin: 0; padding: 20px; }
        .header { text-align: center; border-bottom: 2px solid #333; padding-bottom: 20px; margin-bottom: 30px; }
        .company-info { margin-bottom: 20px; }
        .invoice-details { margin: 20px 0; }
        .items-table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .items-table th, .items-table td { border: 1px solid #ddd; padding: 8px; text-align: right; }
        .items-table th { background-color: #f2f2f2; font-weight: bold; }
        .totals { margin-top: 20px; float: left; }
        .footer { clear: both; margin-top: 40px; text-align: center; border-top: 1px solid #ccc; padding-top: 20px; }
    </style>
</head>
<body>
    <div class="header">
        <h1>{{company.CompanyName}}</h1>
        <p>{{company.Address}}</p>
        <p>هاتف: {{company.Phone}} | البريد الإلكتروني: {{company.Email}}</p>
        <p>الرقم الضريبي: {{company.VatNumber}}</p>
        <h2>فاتورة مبيعات</h2>
    </div>

    <div class="invoice-details">
        <div style="float: right; width: 48%;">
            <h3>معلومات الفاتورة:</h3>
            <p><strong>رقم الفاتورة:</strong> {{header.DocumentNumber}}</p>
            <p><strong>تاريخ الفاتورة:</strong> {{header.DocumentDate}}</p>
        </div>
        
        <div style="float: left; width: 48%;">
            <h3>معلومات العميل:</h3>
            <p><strong>اسم العميل:</strong> {{header.CustomerName}}</p>
            <p><strong>العنوان:</strong> {{header.CustomerAddress}}</p>
        </div>
        <div style="clear: both;"></div>
    </div>

    <table class="items-table">
        <thead>
            <tr>
                <th>م</th>
                <th>اسم الصنف</th>
                <th>الكمية</th>
                <th>سعر الوحدة</th>
                <th>الإجمالي</th>
            </tr>
        </thead>
        <tbody>
            {{#lines}}
            <tr>
                <td>{{@index}}</td>
                <td>{{ItemName}}</td>
                <td>{{Quantity}}</td>
                <td>{{UnitPrice}}</td>
                <td>{{Total}}</td>
            </tr>
            {{/lines}}
        </tbody>
    </table>

    <div class="totals">
        <table style="border-collapse: collapse;">
            <tr>
                <td style="padding: 5px; font-weight: bold;">المجموع الفرعي:</td>
                <td style="padding: 5px;">{{header.NetAmount}}</td>
            </tr>
            <tr>
                <td style="padding: 5px; font-weight: bold;">ضريبة القيمة المضافة:</td>
                <td style="padding: 5px;">{{header.VatAmount}}</td>
            </tr>
            <tr style="border-top: 2px solid #333; font-weight: bold; font-size: 1.1em;">
                <td style="padding: 5px;">الإجمالي:</td>
                <td style="padding: 5px;">{{header.TotalAmount}}</td>
            </tr>
        </table>
    </div>

    <div class="footer">
        <p>شكراً لكم لاختياركم خدماتنا</p>
        <p>تاريخ الطباعة: {{system.PrintDate}} - الساعة: {{system.PrintTime}}</p>
    </div>
</body>
</html>', 
    CONVERT(VARBINARY(32), HASHBYTES('SHA256', N'sample_html_content'))
);

-- إدراج محتوى CSS للقالب
INSERT INTO [dbo].[TemplateContents] (
    [TemplateVersionId], [ContentType], [ContentText], [ContentHash]
) VALUES (
    @VersionId, 'css', N'/* أنماط إضافية لقالب الفاتورة */
@media print {
    body { margin: 0; }
    .header { page-break-after: avoid; }
    .items-table { page-break-inside: avoid; }
}

.highlight {
    background-color: #ffffcc;
    padding: 2px 4px;
}

.currency {
    font-weight: bold;
    color: #2563eb;
}

.total-row {
    font-size: 1.2em;
    font-weight: bold;
    background-color: #f8f9fa;
}',
    CONVERT(VARBINARY(32), HASHBYTES('SHA256', N'sample_css_content'))
);

-- إدراج مصادر البيانات للقالب
INSERT INTO [dbo].[TemplateDataSources] (
    [TemplateVersionId], [Name], [SourceType], [SourceName], [IsMain], [TimeoutSec]
) VALUES 
    (@VersionId, 'header', 'view', 'vw_InvoiceHeader', 1, 30),
    (@VersionId, 'lines', 'view', 'vw_InvoiceLines', 0, 30),
    (@VersionId, 'company', 'view', 'vw_CompanyInfo', 0, 30);

-- إدراج أصول تجريبية
INSERT INTO [dbo].[PrintAssets] (
    [CompanyId], [BranchId], [Name], [MimeType], [Url], [ContentHash], [CreatedAt]
) VALUES 
    (1, NULL, N'شعار الشركة', 'image/png', '/assets/company-logo.png', 
     CONVERT(VARBINARY(32), HASHBYTES('SHA256', N'company_logo')), SYSDATETIME()),
    (1, NULL, N'ختم الشركة', 'image/png', '/assets/company-stamp.png', 
     CONVERT(VARBINARY(32), HASHBYTES('SHA256', N'company_stamp')), SYSDATETIME()),
    (1, NULL, N'أنماط الطباعة', 'text/css', '/assets/print-styles.css', 
     CONVERT(VARBINARY(32), HASHBYTES('SHA256', N'print_styles')), SYSDATETIME());

-- إدراج قالب آخر للإشعار المدين
INSERT INTO [dbo].[PrintTemplates] (
    [CompanyId], [BranchId], [DocumentTypeId], [Name], [Engine], 
    [PaperSize], [Orientation], [Locale], [IsDefault], [Active], 
    [CreatedBy], [CreatedAt]
) VALUES (
    1, NULL, 3, N'قالب الإشعار المدين', 'html',
    'A4', 'P', 'ar-SA', 1, 1,
    1, SYSDATETIME()
);

DECLARE @DebitTemplateId INT = SCOPE_IDENTITY();

-- إدراج إصدار للإشعار المدين
INSERT INTO [dbo].[TemplateVersions] (
    [TemplateId], [VersionNo], [Status], [Notes], 
    [CreatedBy], [CreatedAt], [ActivatedAt]
) VALUES (
    @DebitTemplateId, 1, 'active', N'قالب الإشعار المدين الافتراضي',
    1, SYSDATETIME(), SYSDATETIME()
);

DECLARE @DebitVersionId INT = SCOPE_IDENTITY();

-- إدراج محتوى للإشعار المدين
INSERT INTO [dbo].[TemplateContents] (
    [TemplateVersionId], [ContentType], [ContentText], [ContentHash]
) VALUES (
    @DebitVersionId, 'html', N'<!DOCTYPE html>
<html dir="rtl" lang="ar">
<head>
    <meta charset="UTF-8">
    <title>إشعار مدين</title>
    <style>
        body { font-family: ''Segoe UI'', Tahoma, Arial, sans-serif; direction: rtl; margin: 20px; }
        .header { text-align: center; border-bottom: 2px solid #333; padding-bottom: 15px; margin-bottom: 20px; }
        .content { margin: 20px 0; }
        .amount { font-size: 1.5em; font-weight: bold; color: #d32f2f; text-align: center; }
    </style>
</head>
<body>
    <div class="header">
        <h1>{{company.CompanyName}}</h1>
        <h2>إشعار مدين</h2>
    </div>
    
    <div class="content">
        <p><strong>رقم الإشعار:</strong> {{header.DocumentNumber}}</p>
        <p><strong>التاريخ:</strong> {{header.DocumentDate}}</p>
        <p><strong>العميل:</strong> {{header.CustomerName}}</p>
        
        <div class="amount">
            <p>المبلغ: {{header.TotalAmount}} ريال</p>
        </div>
        
        <p><strong>السبب:</strong> {{header.Reason}}</p>
    </div>
</body>
</html>',
    CONVERT(VARBINARY(32), HASHBYTES('SHA256', N'debit_note_content'))
);

PRINT N'تم إدراج البيانات التجريبية لنظام إدارة قوالب الطباعة بنجاح';
GO
