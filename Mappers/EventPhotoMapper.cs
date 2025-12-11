using IEEEBackend.Dtos;
using IEEEBackend.Models;

namespace IEEEBackend.Mappers;

public static class EventPhotoMapper
{
    public static EventPhotoDto ToEventPhotoDto(EventPhoto eventPhoto)
    {
        return new EventPhotoDto
        {
            Id = eventPhoto.Id,
            EventId = eventPhoto.EventId,
            ImageUrl = $"/{eventPhoto.ImageUrl}",
            IsCover = eventPhoto.IsCover,
            CreatedAt = eventPhoto.CreatedAt,
            UpdatedAt = eventPhoto.UpdatedAt
        };
    }

    public static IEnumerable<EventPhotoDto> ToEventPhotoDtoList(IEnumerable<EventPhoto> eventPhotos)
    {
        return eventPhotos.Select(ToEventPhotoDto);
    }
}

