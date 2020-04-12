namespace DocumentUpload.Services.Files
{
    public class FileValidationOptions
    {
        public int MaxSize { get; set; }
        public string[] Extensions { get; set; }
    }
}