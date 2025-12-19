namespace IEEEBackend.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder);
    Task DeleteFileAsync(string filePath);
    string GetPublicUrl(string filePath);
}
