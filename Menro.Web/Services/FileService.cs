public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveProfileImageAsync(IFormFile file, string? oldFileName = null)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "img", "profile");
        Directory.CreateDirectory(uploadDir);

        // حذف عکس قبلی
        if (!string.IsNullOrEmpty(oldFileName))
        {
            var oldPath = Path.Combine(uploadDir, oldFileName);
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }
}
