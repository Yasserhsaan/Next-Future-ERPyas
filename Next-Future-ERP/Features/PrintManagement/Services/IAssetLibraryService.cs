using Next_Future_ERP.Features.PrintManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.PrintManagement.Services
{
    /// <summary>
    /// خدمة مكتبة الأصول - إدارة الأصول: رفع/تحديث/حذف/ضغط الصور
    /// </summary>
    public interface IAssetLibraryService
    {
        /// <summary>
        /// الحصول على جميع الأصول
        /// </summary>
        Task<List<AssetInfo>> GetAssetsAsync(int? companyId = null, int? branchId = null, string? assetType = null);

        /// <summary>
        /// الحصول على أصل واحد
        /// </summary>
        Task<PrintAsset?> GetAssetByIdAsync(int assetId);

        /// <summary>
        /// رفع أصل جديد
        /// </summary>
        Task<PrintAsset> UploadAssetAsync(AssetUploadRequest request);

        /// <summary>
        /// تحديث أصل موجود
        /// </summary>
        Task<PrintAsset> UpdateAssetAsync(int assetId, AssetUploadRequest request);

        /// <summary>
        /// حذف أصل
        /// </summary>
        Task<bool> DeleteAssetAsync(int assetId);

        /// <summary>
        /// الحصول على بيانات الأصل للتحميل
        /// </summary>
        Task<(byte[] Data, string MimeType, string FileName)?> GetAssetDataAsync(int assetId);

        /// <summary>
        /// ضغط الصورة
        /// </summary>
        Task<byte[]> CompressImageAsync(byte[] imageData, int maxWidth = 800, int quality = 85);

        /// <summary>
        /// التحقق من نوع الملف المسموح
        /// </summary>
        bool IsAllowedFileType(string fileName, string mimeType);

        /// <summary>
        /// الحصول على الأنواع المسموحة
        /// </summary>
        List<string> GetAllowedMimeTypes();

        /// <summary>
        /// حساب بصمة الملف
        /// </summary>
        byte[] ComputeFileHash(byte[] data);
    }

    /// <summary>
    /// معلومات الأصل للعرض
    /// </summary>
    public class AssetInfo
    {
        public int AssetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsImage { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? BranchName { get; set; }
    }

    /// <summary>
    /// طلب رفع الأصل
    /// </summary>
    public class AssetUploadRequest
    {
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public string? AssetType { get; set; }
        public bool CompressIfImage { get; set; } = true;
    }
}
