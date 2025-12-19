using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class EventPhotoRepository : IEventPhotoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly IConfiguration _configuration;

    public EventPhotoRepository(
        ApplicationDbContext context,
        IStorageService storageService,
        IConfiguration configuration)
    {
        _context = context;
        _storageService = storageService;
        _configuration = configuration;
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
        ValidateFile(file);
        var imageUrl = await UploadToSupabaseAsync(eventId, file, isCover: true);

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
            ValidateFile(file);
            var imageUrl = await UploadToSupabaseAsync(eventId, file, isCover: false);

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

        // Delete from Supabase Storage
        await _storageService.DeleteFileAsync(eventPhoto.ImageUrl);

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

    private async Task<string> UploadToSupabaseAsync(int eventId, IFormFile file, bool isCover)
    {
        var subFolder = isCover ? "cover" : "photos";
        var folder = $"events/{eventId}/{subFolder}";

        using var stream = file.OpenReadStream();
        var filePath = await _storageService.UploadFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            folder
        );

        // Return the full public URL
        return _storageService.GetPublicUrl(filePath);
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
