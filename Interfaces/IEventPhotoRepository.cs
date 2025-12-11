using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface IEventPhotoRepository
{
    Task<EventPhoto?> GetCoverPhotoByEventIdAsync(int eventId);
    Task<IEnumerable<EventPhoto>> GetEventPhotosByEventIdAsync(int eventId);
    Task<EventPhoto?> GetByIdAsync(int photoId);
    Task<EventPhoto> CreateCoverPhotoAsync(int eventId, IFormFile file);
    Task<IEnumerable<EventPhoto>> CreateEventPhotosAsync(int eventId, IEnumerable<IFormFile> files);
    Task<bool> DeleteAsync(int photoId);
    Task<bool> DeleteCoverPhotoByEventIdAsync(int eventId);
}

