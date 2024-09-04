namespace MTSTeskTask.Models
{
    public interface IImageConverter
    {
        bool ConvertImageFormat(IFormFile imageFile, string filePath, string foramat);
        Task<string> ConvertImageToBase64(string filePath);  
    }
}
