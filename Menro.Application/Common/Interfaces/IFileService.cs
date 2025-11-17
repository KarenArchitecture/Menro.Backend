using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<string> UploadProfileImageAsync(IFormFile file, string? oldFileName = null);
    Task<string> UploadSvgAsync(IFormFile file);
    Task<string> UploadAdImageAsync(IFormFile file);
    Task<string> UploadFoodImageAsync(IFormFile file);
    bool DeleteFoodImage(string fileName);
    bool DeleteIcon(string fileName);
}