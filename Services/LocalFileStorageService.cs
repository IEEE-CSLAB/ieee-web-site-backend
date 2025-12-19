using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace IEEEBackend.Services;

/// <summary>
/// Local file storage implementation using wwwroot/uploads directory.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _uploadRootPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;

        // Get upload root path from config (default: "uploads")
        // WebRootPath already points to wwwroot, so we just append the upload path
        var configPath = _configuration["FileUpload:UploadPath"] ?? "uploads";
        _uploadRootPath = Path.Combine(
            _environment.WebRootPath ?? _environment.ContentRootPath,
            configPath);

        // Ensure root directory exists
        if (!Directory.Exists(_uploadRootPath))
        {
            Directory.CreateDirectory(_uploadRootPath);
        }

        // Get validation settings
        _maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880); // 5MB default
        _allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };
    }

    public async Task<string> SaveFileAsync(string container, IFormFile file, CancellationToken cancellationToken = default)
    {
        // Validate file
        ValidateFile(file);

        // Generate unique filename
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";

        // Build full path: uploadRoot/container/...
        var containerPath = Path.Combine(_uploadRootPath, container);
        if (!Directory.Exists(containerPath))
        {
            Directory.CreateDirectory(containerPath);
        }

        var filePath = Path.Combine(containerPath, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Return relative path for database storage
        // Format: "uploads/{container}/..."
        var relativePath = Path.Combine("uploads", container, fileName)
            .Replace('\\', '/');

        return relativePath;
    }

    public async Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        // Remove leading slash if present
        var cleanPath = relativePath.TrimStart('/');

        // Build full path
        var fullPath = Path.Combine(
            _environment.WebRootPath ?? _environment.ContentRootPath,
            cleanPath);

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.");
        }

        if (file.Length > _maxFileSize)
        {
            throw new ArgumentException(
                $"File size exceeds the maximum allowed size of {_maxFileSize / 1024 / 1024}MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException(
                $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _allowedExtensions)}");
        }
    }
}

