using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using MTSTeskTask.Models;
using Microsoft.Extensions.Logging;

namespace MTSTeskTask.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : Controller
    {
        private readonly ILogger<ImageController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        private readonly IImageConverter _imageConverter;

        public ImageController(ILogger<ImageController> logger, IHttpClientFactory httpClientFactory, IImageConverter imageConverter)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _imageConverter = imageConverter;

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }


        [HttpPost("convert")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ConvertAndUploadImage([FromForm] IFormFile imageFile, [FromForm] string targetFormat = "jpg")
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest("Image file is required.");
                }

                string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                string targetFileName = $"{fileName}.{targetFormat}";
                string targetFilePath = Path.Combine(_storagePath, targetFileName);

                bool conversionSuccess = _imageConverter.ConvertImageFormat(imageFile, targetFilePath, targetFormat);
                if (!conversionSuccess)
                {
                    return BadRequest("Unsupported image format.");
                }

                string base64Image = await _imageConverter.ConvertImageToBase64(targetFilePath);

                var httpClient = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(new { image = base64Image }), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://httpbin.org/post", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                string logFileName = $"{fileName}_log.txt";
                string logFilePath = Path.Combine(_storagePath, logFileName);
                await System.IO.File.WriteAllTextAsync(logFilePath, $"{response.StatusCode}\n{responseContent}");

                return Ok(new { FileName = targetFileName, Result = "Success", Error = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during image conversion and upload.");
                return StatusCode(500, new { FileName = "", Result = "Failed", Error = ex.Message });
            }
        }

        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_storagePath, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file retrieval.");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_storagePath, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File not found.");
                }

                System.IO.File.Delete(filePath);
                return Ok(new { Result = "Success", Error = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file deletion.");
                return StatusCode(500, new { Result = "Failed", Error = ex.Message });
            }
        }
    }
}
