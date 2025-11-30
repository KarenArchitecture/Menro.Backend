public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /*--------------------------------------------*/
    /*            PRIVATE GENERIC UPLOADER        */
    /*--------------------------------------------*/
    private async Task<string> SaveFileAsync(
        IFormFile file,
        string folder,
        string? oldFileName = null)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "img", folder);
        Directory.CreateDirectory(uploadDir);

        // delete old file (if exists)
        if (!string.IsNullOrEmpty(oldFileName))
        {
            var oldPath = Path.Combine(uploadDir, oldFileName);
            if (File.Exists(oldPath))
                File.Delete(oldPath);
        }

        // generate new file name
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        // save file
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    /*--------------------------------------------*/
    /*                   ICONS (SVG)              */
    /*--------------------------------------------*/
    public async Task<string> UploadSvgAsync(IFormFile file)
    {
        var uploadDir = Path.Combine(_env.WebRootPath ?? "", "icons");
        Directory.CreateDirectory(uploadDir);

        // we do NOT generate GUID here because SVG icons have meaningful names
        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadDir, fileName);

        if (File.Exists(filePath))
            throw new InvalidOperationException("Icon already exists.");

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    public bool DeleteIcon(string fileName)
    {
        try
        {
            var uploadDir = Path.Combine(_env.WebRootPath ?? "", "icons");
            var safeName = Path.GetFileName(fileName);
            var path = Path.Combine(uploadDir, safeName);

            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /*--------------------------------------------*/
    /*                 PROFILE IMAGE              */
    /*--------------------------------------------*/
    public Task<string> UploadProfileImageAsync(IFormFile file, string? oldFileName = null)
        => SaveFileAsync(file, "profile", oldFileName);

    /*--------------------------------------------*/
    /*                  AD IMAGE                  */
    /*--------------------------------------------*/
    public Task<string> UploadAdImageAsync(IFormFile file)
        => SaveFileAsync(file, "adBanner");

    /*--------------------------------------------*/
    /*                  FOOD IMAGE                */
    /*--------------------------------------------*/
    public Task<string> UploadFoodImageAsync(IFormFile file)
        => SaveFileAsync(file, "food");

    public bool DeleteFoodImage(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName)) return false;

            var uploadDir = Path.Combine(_env.WebRootPath, "img", "food");
            var path = Path.Combine(uploadDir, fileName);

            if (!File.Exists(path)) return false;

            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /*--------------------------------------------*/
    /*            RESTAURANT IMAGES              */
    /*--------------------------------------------*/
    public Task<string> UploadRestaurantHomeBannerAsync(IFormFile file, string? oldFileName = null)
        => SaveFileAsync(file, Path.Combine("restaurants", "home"), oldFileName);

    public Task<string> UploadRestaurantShopBannerAsync(IFormFile file, string? oldFileName = null)
        => SaveFileAsync(file, Path.Combine("restaurants", "shop"), oldFileName);

    public Task<string> UploadRestaurantLogoAsync(IFormFile file, string? oldFileName = null)
        => SaveFileAsync(file, Path.Combine("restaurants", "logo"), oldFileName);
}
