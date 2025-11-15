using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<string> SaveProfileImageAsync(IFormFile file, string? oldFileName = null);
    Task<string> UploadSvgAsync(IFormFile file);
    bool DeleteIcon(string fileName);
}