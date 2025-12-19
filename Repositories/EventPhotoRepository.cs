using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using IEEEBackend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class EventPhotoRepository : IEventPhotoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public EventPhotoRepository(
        ApplicationDbContext context,
        IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
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

        // Save file using FileStorageService
        var container = $"events/{eventId}/cover";
        var imageUrl = await _fileStorage.SaveFileAsync(container, file);

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
            var container = $"events/{eventId}/photos";
            var imageUrl = await _fileStorage.SaveFileAsync(container, file);

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

        // Delete physical file using FileStorageService
        await _fileStorage.DeleteFileAsync(eventPhoto.ImageUrl);

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
}

