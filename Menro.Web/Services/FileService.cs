public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    // images/profile
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

    // icons
    public async Task<string> UploadSvgAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath ?? string.Empty, "icons");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = Path.GetFileName(file.FileName);

        var filePath = Path.Combine(uploadsFolder, fileName);
        if (File.Exists(filePath))
            throw new InvalidOperationException("File already exists.");

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return fileName;
    }
    public bool DeleteIcon(string fileName)
    {
        try
        {
            var iconsFolder = Path.Combine(_env.WebRootPath ?? "", "icons");
            var safeName = Path.GetFileName(fileName);
            var filePath = Path.Combine(iconsFolder, safeName);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
