using Microsoft.AspNetCore.Http;

public interface IFileStorageService
{
    Task<string> SaveProfileImageAsync(IFormFile file, string? oldFileName = null);
}