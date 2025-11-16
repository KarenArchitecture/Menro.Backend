using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<string> UploadProfileImageAsync(IFormFile file, string? oldFileName = null);
    Task<string> UploadSvgAsync(IFormFile file);
    Task<string> UploadAdImageAsync(IFormFile file);
    bool DeleteIcon(string fileName);
}