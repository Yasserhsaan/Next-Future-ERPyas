# نظام إدارة الطباعة (Print Management System)

## نظرة عامة

تم تطوير نظام إدارة الطباعة بشكل كامل ليتكامل مع نظام Next Future ERP. يوفر النظام إدارة شاملة لقوالب الطباعة مع دعم أنواع مختلفة من المحركات والمعاينة المباشرة.

## المكونات المطورة

### 1. النماذج (Models)
- **PrintTemplate**: القالب الأساسي للطباعة
- **TemplateVersion**: إدارة إصدارات القوالب
- **TemplateContent**: محتوى القوالب (HTML, CSS, Liquid, إلخ)
- **TemplateDataSource**: مصادر البيانات للقوالب
- **PrintJob**: سجلات الطباعة والمهام
- **PrintAsset**: أصول الطباعة (صور، خطوط، إلخ)

### 2. الخدمات (Services)
- **ITemplateCatalogService**: إدارة كتالوج القوالب
- **TemplateCatalogService**: تنفيذ كامل لجميع عمليات CRUD
- **IVersioningService**: إدارة إصدارات القوالب
- **VersioningService**: تنفيذ كامل لإدارة الإصدارات
- **IContentService**: إدارة محتوى القوالب
- **ContentService**: تنفيذ كامل لإدارة المحتوى
- **IRenderPreviewService**: خدمة المعاينة والطباعة
- **RenderPreviewService**: تنفيذ كامل للمعاينة والطباعة
- **PrintManagementSeedDataService**: إضافة البيانات التجريبية

### 3. واجهات المستخدم (Views)
- **TemplateLibraryView**: مكتبة القوالب
- **TemplateWorkspaceView**: مساحة عمل القوالب مع تحرير كامل

### 4. ViewModels
- **TemplateLibraryViewModel**: إدارة مكتبة القوالب
- **TemplateWorkspaceViewModel**: إدارة مساحة عمل القوالب مع أوامر كاملة

## الميزات المطورة

### ✅ إدارة القوالب
- إنشاء قوالب جديدة
- تعديل القوالب الموجودة
- حذف القوالب
- نسخ القوالب
- تعيين القوالب الافتراضية

### ✅ إدارة الإصدارات
- إنشاء إصدارات جديدة
- تفعيل الإصدارات
- تتبع تاريخ التغييرات
- إدارة الملاحظات

### ✅ إدارة المحتوى
- دعم HTML
- دعم CSS
- دعم Liquid Templates
- دعم Razor Templates
- دعم Handlebars
- دعم FreeMarker

### ✅ المعاينة والطباعة
- معاينة HTML مباشرة
- معاينة مع بيانات تجريبية
- التحقق من صحة القوالب
- طباعة PDF
- دعم تنسيقات متعددة

### ✅ مصادر البيانات
- ربط القوالب بقواعد البيانات
- دعم Views و Procedures
- اختبار الاتصال
- إدارة المهلة الزمنية

## قاعدة البيانات

تم إضافة الجداول التالية:
- `PrintTemplates`
- `TemplateVersions`
- `TemplateContents`
- `TemplateDataSources`
- `PrintJobs`
- `PrintAssets`

## البيانات التجريبية

تم إضافة بيانات تجريبية شاملة تشمل:
- قوالب لسندات القبض والدفع
- قوالب للفواتير
- قوالب لأوامر الشراء
- أصول الطباعة (شعارات، خطوط)
- محتوى HTML و CSS جاهز

## كيفية الاستخدام

### 1. إضافة البيانات التجريبية
```csharp
var seedService = new PrintManagementSeedDataService(context);
await seedService.SeedPrintManagementDataAsync();
```

### 2. استخدام خدمات القوالب
```csharp
// الحصول على القوالب
var templates = await catalogService.GetTemplatesAsync(companyId: 1, documentTypeId: 1);

// إنشاء قالب جديد
var newTemplate = await catalogService.CreateTemplateAsync(template);

// معاينة القالب
var preview = await previewService.RenderPreviewAsync(templateVersionId);
```

### 3. ربط الخدمات في DI Container
```csharp
services.AddScoped<ITemplateCatalogService, TemplateCatalogService>();
services.AddScoped<IVersioningService, VersioningService>();
services.AddScoped<IContentService, ContentService>();
services.AddScoped<IRenderPreviewService, RenderPreviewService>();
```

## الأوامر المتاحة

### في TemplateWorkspaceViewModel:
- `PreviewTemplateAsyncCommand`: معاينة القالب
- `PreviewWithSampleDataAsyncCommand`: معاينة مع بيانات تجريبية
- `ValidateTemplateAsyncCommand`: التحقق من صحة القالب
- `PrintTemplateAsyncCommand`: طباعة القالب
- `CreateNewVersionAsyncCommand`: إنشاء إصدار جديد
- `ActivateVersionAsyncCommand`: تفعيل الإصدار
- `SaveHtmlContentAsyncCommand`: حفظ محتوى HTML
- `SaveCssContentAsyncCommand`: حفظ محتوى CSS
- `DeleteContentAsyncCommand`: حذف المحتوى
- `AddDataSourceAsyncCommand`: إضافة مصدر بيانات
- `SetMainDataSourceAsyncCommand`: تعيين مصدر البيانات الرئيسي

## التحسينات المستقبلية

1. **دعم محركات إضافية**: Jinja2, Mustache
2. **تحسين المعاينة**: WebView2 بدلاً من WebBrowser
3. **دعم PDF متقدم**: iTextSharp أو Puppeteer
4. **تحسين الأداء**: Caching للقوالب
5. **دعم التصميم المرئي**: WYSIWYG Editor
6. **دعم التصدير/الاستيراد**: JSON/XML
7. **دعم الإشعارات**: Toast Notifications
8. **دعم السجلات**: Audit Trail

## ملاحظات مهمة

- جميع الخدمات تدعم معالجة الأخطاء بشكل مناسب
- تم تطبيق مبادئ SOLID في التصميم
- دعم كامل للغة العربية
- واجهة مستخدم متجاوبة وجميلة
- كود نظيف ومُوثق جيداً

## الملفات المهمة

- `Models/`: جميع نماذج البيانات
- `Services/`: جميع الخدمات والواجهات
- `Views/`: واجهات المستخدم
- `ViewModels/`: نماذج العرض
- `Converters/`: محولات البيانات
- `README.md`: هذا الملف

---

تم تطوير هذا النظام بواسطة فريق Next Future ERP
تاريخ التطوير: سبتمبر 2025
