using Microsoft.AspNetCore.Mvc;

namespace Menro.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public FileController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ICONS ✅
        [HttpPost("upload-icon")]
        public async Task<IActionResult> UploadSvg([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                /*
                 یافتن مسیر پوشه‌ی
                 wwwroot/icons
                 با استفاده از
                 IWebHostEnvironment
                */
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? string.Empty, "icons");

                // اگر پوشه وجود ندارد، بسازش
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // نام فایل
                var fileName = Path.GetFileName(file.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // ذخیره فایل
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // برگردوندن فقط نام فایل + فرمت برای ذخیره در دیتابیس
                return Ok(new { fileName });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "File upload failed", error = ex.Message });
            }
        }

        [HttpDelete("delete-icon")]
        public IActionResult DeleteIcon([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("File name is required.");

            try
            {
                // مسیر پوشه‌ی icons داخل wwwroot
                var iconsFolder = Path.Combine(_env.WebRootPath ?? string.Empty, "icons");

                // فقط نام فایل را مجاز می‌کنیم (برای جلوگیری از حمله‌های مسیر نسبی ../)
                var safeFileName = Path.GetFileName(fileName);

                var filePath = Path.Combine(iconsFolder, safeFileName);

                // بررسی وجود فایل
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "File not found." });

                // حذف فایل
                System.IO.File.Delete(filePath);

                return Ok(new { message = "File deleted successfully.", fileName = safeFileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting file.", error = ex.Message });
            }
        }

    }
}
