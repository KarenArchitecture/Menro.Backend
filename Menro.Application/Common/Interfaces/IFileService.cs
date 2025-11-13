using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<string> SaveProfileImageAsync(IFormFile file, string? oldFileName = null);
}