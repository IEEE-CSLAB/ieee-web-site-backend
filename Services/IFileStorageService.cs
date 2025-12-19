using Microsoft.AspNetCore.Http;

namespace IEEEBackend.Services;

/// <summary>
/// Interface for file storage operations.
/// Allows switching between local storage and cloud storage (S3, Azure Blob, etc.) in the future.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage and returns the relative path for database storage.
    /// </summary>
    /// <param name="container">Container/folder name (e.g., "events", "blogs", "committees")</param>
    /// <param name="file">The file to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative path (e.g., "uploads/events/8/cover/abc123.jpg")</returns>
    Task<string> SaveFileAsync(string container, IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage using its relative path.
    /// </summary>
    /// <param name="relativePath">Relative path of the file (e.g., "uploads/events/8/cover/abc123.jpg")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file was deleted, false if file didn't exist</returns>
    Task<bool> DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
}

