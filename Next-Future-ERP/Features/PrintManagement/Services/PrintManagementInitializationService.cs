using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة تهيئة نظام إدارة الطباعة
    /// </summary>
    public class PrintManagementInitializationService
    {
        private readonly AppDbContext _context;
        private readonly PrintManagementSeedDataService _seedDataService;

        public PrintManagementInitializationService(AppDbContext context)
        {
            _context = context;
            _seedDataService = new PrintManagementSeedDataService(context);
        }

        /// <summary>
        /// تهيئة نظام إدارة الطباعة مع البيانات التجريبية
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                // التحقق من الاتصال بقاعدة البيانات
                if (!await _context.Database.CanConnectAsync())
                {
                    MessageBox.Show("❌ لا يمكن الاتصال بقاعدة البيانات", 
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // التحقق من وجود الجداول
                await EnsureTablesExistAsync();

                // إضافة البيانات التجريبية إذا لم تكن موجودة
                await _seedDataService.SeedPrintManagementDataAsync();

                MessageBox.Show("✅ تم تهيئة نظام إدارة الطباعة بنجاح", 
                    "نجح", MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ خطأ في تهيئة نظام إدارة الطباعة:\n{ex.Message}", 
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// التحقق من وجود جداول PrintManagement
        /// </summary>
        private async Task EnsureTablesExistAsync()
        {
            try
            {
                // محاولة الوصول للجداول للتأكد من وجودها
                await _context.PrintTemplates.CountAsync();
                await _context.TemplateVersions.CountAsync();
                await _context.TemplateContents.CountAsync();
                await _context.TemplateDataSources.CountAsync();
                await _context.PrintJobs.CountAsync();
                await _context.PrintAssets.CountAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"جداول PrintManagement غير موجودة أو تالفة: {ex.Message}");
            }
        }

        /// <summary>
        /// التحقق من حالة النظام
        /// </summary>
        public async Task<SystemStatus> CheckSystemStatusAsync()
        {
            try
            {
                var status = new SystemStatus();

                // فحص الاتصال بقاعدة البيانات
                status.DatabaseConnected = await _context.Database.CanConnectAsync();

                if (status.DatabaseConnected)
                {
                    // فحص الجداول
                    try
                    {
                        await EnsureTablesExistAsync();
                        status.TablesExist = true;
                    }
                    catch
                    {
                        status.TablesExist = false;
                    }

                    // فحص البيانات
                    if (status.TablesExist)
                    {
                        status.TemplatesCount = await _context.PrintTemplates.CountAsync();
                        status.VersionsCount = await _context.TemplateVersions.CountAsync();
                        status.ContentsCount = await _context.TemplateContents.CountAsync();
                        status.AssetsCount = await _context.PrintAssets.CountAsync();
                        status.JobsCount = await _context.PrintJobs.CountAsync();
                        
                        status.HasSampleData = status.TemplatesCount > 0;
                    }
                }

                return status;
            }
            catch (Exception ex)
            {
                return new SystemStatus 
                { 
                    DatabaseConnected = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }
    }

    /// <summary>
    /// حالة نظام إدارة الطباعة
    /// </summary>
    public class SystemStatus
    {
        public bool DatabaseConnected { get; set; }
        public bool TablesExist { get; set; }
        public bool HasSampleData { get; set; }
        public int TemplatesCount { get; set; }
        public int VersionsCount { get; set; }
        public int ContentsCount { get; set; }
        public int AssetsCount { get; set; }
        public int JobsCount { get; set; }
        public string? ErrorMessage { get; set; }

        public bool IsHealthy => DatabaseConnected && TablesExist;
        public bool IsReady => IsHealthy && HasSampleData;

        public string StatusMessage
        {
            get
            {
                if (!DatabaseConnected)
                    return "❌ لا يمكن الاتصال بقاعدة البيانات";
                
                if (!TablesExist)
                    return "❌ جداول PrintManagement غير موجودة";
                
                if (!HasSampleData)
                    return "⚠️ لا توجد بيانات تجريبية";
                
                return $"✅ النظام جاهز - {TemplatesCount} قالب";
            }
        }
    }
}
