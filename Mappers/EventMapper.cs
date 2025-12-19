using IEEEBackend.Dtos;
using IEEEBackend.Models;

namespace IEEEBackend.Mappers;

public static class EventMapper
{
    public static EventDto ToEventDto(Event eventEntity)
    {
        var coverPhoto = eventEntity.EventPhotos?.FirstOrDefault(ep => ep.IsCover);
        
        return new EventDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            CoverImageUrl = coverPhoto != null ? $"/{coverPhoto.ImageUrl}" : null,
            StartDate = eventEntity.StartDate,
            EndDate = eventEntity.EndDate,
            Location = eventEntity.Location,
            Quota = eventEntity.Quota,
            IsImportant = eventEntity.IsImportant,
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
            Committees = eventEntity.EventCommittees?
                .Where(ec => ec.Committee != null)
                .Select(ec => new CommitteeDto
                {
                    Id = ec.Committee.Id,
                    Name = ec.Committee.Name,
                    Description = ec.Committee.Description,
                    LogoUrl = ec.Committee.LogoUrl
                })
                .ToList(),
            Photos = eventEntity.EventPhotos != null 
                ? EventPhotoMapper.ToEventPhotoDtoList(eventEntity.EventPhotos).ToList()
                : null
        };
    }

    public static Event ToEvent(CreateEventDto createEventDto)
    {
        var eventEntity = new Event
        {
            Title = createEventDto.Title,
            Description = createEventDto.Description,
            StartDate = createEventDto.StartDate,
            EndDate = createEventDto.EndDate,
            Location = createEventDto.Location,
            Quota = createEventDto.Quota,
            IsImportant = createEventDto.IsImportant
        };

        if (createEventDto.CommitteeIds != null && createEventDto.CommitteeIds.Any())
        {
            eventEntity.EventCommittees = createEventDto.CommitteeIds
                .Select(committeeId => new EventCommittee
                {
                    CommitteeId = committeeId
                })
                .ToList();
        }

        return eventEntity;
    }

    public static void UpdateEventFromDto(Event eventEntity, UpdateEventDto updateEventDto)
    {
        eventEntity.Title = updateEventDto.Title;
        eventEntity.Description = updateEventDto.Description;
        eventEntity.StartDate = updateEventDto.StartDate;
        eventEntity.EndDate = updateEventDto.EndDate;
        eventEntity.Location = updateEventDto.Location;
        eventEntity.Quota = updateEventDto.Quota;
        eventEntity.IsImportant = updateEventDto.IsImportant;

        // Update EventCommittees if provided
        if (updateEventDto.CommitteeIds != null)
        {
            // Ensure collection is initialized
            if (eventEntity.EventCommittees == null)
            {
                eventEntity.EventCommittees = new List<EventCommittee>();
            }
            else
            {
                // Remove existing EventCommittees
                eventEntity.EventCommittees.Clear();
            }

            // Add new EventCommittees
            if (updateEventDto.CommitteeIds.Any())
            {
                eventEntity.EventCommittees = updateEventDto.CommitteeIds
                    .Select(committeeId => new EventCommittee
                    {
                        CommitteeId = committeeId
                    })
                    .ToList();
            }
        }
    }
}

