// Menro.Application/Common/Media/MediaStorageOptions.cs
namespace Menro.Application.Common.Media
{
    public class MediaStorageOptions
    {
        public required string RootPath { get; set; }   // مسیر فیزیکی ریشه
        public required string BaseUrl { get; set; }     // برای ساخت URL عمومی
    }
}
