using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class EventPhotoRepository : IEventPhotoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;

    public EventPhotoRepository(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _configuration = configuration;
        _uploadPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, 
            _configuration["FileUpload:UploadPath"] ?? "wwwroot/uploads/events");
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<EventPhoto?> GetCoverPhotoByEventIdAsync(int eventId)
    {
        return await _context.EventPhotos
            .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.IsCover);
    }

    public async Task<IEnumerable<EventPhoto>> GetEventPhotosByEventIdAsync(int eventId)
    {
        return await _context.EventPhotos
            .Where(ep => ep.EventId == eventId && !ep.IsCover)
            .OrderBy(ep => ep.CreatedAt)
            .ToListAsync();
    }

    public async Task<EventPhoto?> GetByIdAsync(int photoId)
    {
        return await _context.EventPhotos
            .Include(ep => ep.Event)
            .FirstOrDefaultAsync(ep => ep.Id == photoId);
    }

    public async Task<EventPhoto> CreateCoverPhotoAsync(int eventId, IFormFile file)
    {
        // Validate event exists
        var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            throw new InvalidOperationException($"Event with ID {eventId} not found.");
        }

        // Delete existing cover photo if exists
        var existingCover = await GetCoverPhotoByEventIdAsync(eventId);
        if (existingCover != null)
        {
            await DeleteAsync(existingCover.Id);
        }

        // Validate and save file
        var imageUrl = await SaveFileAsync(eventId, file, isCover: true);

        var eventPhoto = new EventPhoto
        {
            EventId = eventId,
            ImageUrl = imageUrl,
            IsCover = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EventPhotos.Add(eventPhoto);
        await _context.SaveChangesAsync();

        return eventPhoto;
    }

    public async Task<IEnumerable<EventPhoto>> CreateEventPhotosAsync(int eventId, IEnumerable<IFormFile> files)
    {
        // Validate event exists
        var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            throw new InvalidOperationException($"Event with ID {eventId} not found.");
        }

        var eventPhotos = new List<EventPhoto>();

        foreach (var file in files)
        {
            var imageUrl = await SaveFileAsync(eventId, file, isCover: false);

            var eventPhoto = new EventPhoto
            {
                EventId = eventId,
                ImageUrl = imageUrl,
                IsCover = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            eventPhotos.Add(eventPhoto);
        }

        _context.EventPhotos.AddRange(eventPhotos);
        await _context.SaveChangesAsync();

        return eventPhotos;
    }

    public async Task<bool> DeleteAsync(int photoId)
    {
        var eventPhoto = await _context.EventPhotos.FindAsync(photoId);
        if (eventPhoto == null)
        {
            return false;
        }

        // Delete physical file
        var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, eventPhoto.ImageUrl);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.EventPhotos.Remove(eventPhoto);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCoverPhotoByEventIdAsync(int eventId)
    {
        var coverPhoto = await GetCoverPhotoByEventIdAsync(eventId);
        if (coverPhoto == null)
        {
            return false;
        }

        return await DeleteAsync(coverPhoto.Id);
    }

    private async Task<string> SaveFileAsync(int eventId, IFormFile file, bool isCover)
    {
        // Validate file
        ValidateFile(file);

        // Generate unique filename
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var eventFolder = Path.Combine(_uploadPath, eventId.ToString());
        
        if (!Directory.Exists(eventFolder))
        {
            Directory.CreateDirectory(eventFolder);
        }

        var subFolder = isCover ? "cover" : "photos";
        var finalFolder = Path.Combine(eventFolder, subFolder);
        
        if (!Directory.Exists(finalFolder))
        {
            Directory.CreateDirectory(finalFolder);
        }

        var filePath = Path.Combine(finalFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for database storage
        var relativePath = Path.Combine(_configuration["FileUpload:UploadPath"] ?? "wwwroot/uploads/events", 
            eventId.ToString(), subFolder, fileName)
            .Replace('\\', '/');

        return relativePath;
    }

    private void ValidateFile(IFormFile file)
    {
        var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880); // 5MB default
        var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.");
        }

        if (file.Length > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {maxFileSize / 1024 / 1024}MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
        }
    }
}

