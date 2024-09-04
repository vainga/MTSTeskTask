using System.Drawing.Imaging;
using System.Drawing;

namespace MTSTeskTask.Models
{
    public class ImageConverter : IImageConverter
    {
        public bool ConvertImageFormat(IFormFile imageFile, string filePath, string format)
        {
            try
            {
                using var image = Image.FromStream(imageFile.OpenReadStream());
                using var stream = new FileStream(filePath, FileMode.Create);

                switch (format.ToLower())
                {
                    case "jpg":
                    case "jpeg":
                        image.Save(stream, ImageFormat.Jpeg);
                        break;
                    case "bmp":
                        image.Save(stream, ImageFormat.Bmp);
                        break;
                    case "png":
                        image.Save(stream, ImageFormat.Png);
                        break;
                    default:
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> ConvertImageToBase64(string filePath)
        {
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return Convert.ToBase64String(imageBytes);
        }

    }
}
