using System.Net.Http.Headers;
using System.Text;
using IEEEBackend.Interfaces;

namespace IEEEBackend.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;
    private readonly string _bucketName = "uploads";

    public SupabaseStorageService(IConfiguration configuration)
    {
        _supabaseUrl = configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url is not configured");
        _serviceKey = configuration["Supabase:ServiceKey"]
            ?? throw new InvalidOperationException("Supabase:ServiceKey is not configured");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _serviceKey);
        _httpClient.DefaultRequestHeaders.Add("apikey", _serviceKey);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = string.IsNullOrEmpty(folder) ? uniqueName : $"{folder}/{uniqueName}";

        var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{filePath}";

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        using var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to upload file to Supabase: {error}");
        }

        return filePath;
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{filePath}";

        var response = await _httpClient.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            // Log but don't throw - file might already be deleted
            Console.WriteLine($"Warning: Failed to delete file from Supabase: {error}");
        }
    }

    public string GetPublicUrl(string filePath)
    {
        return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{filePath}";
    }
}
