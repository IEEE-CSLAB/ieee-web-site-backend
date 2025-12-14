using IEEEBackend.Dtos;
using IEEEBackend.Interfaces;
using IEEEBackend.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventPhotoRepository _eventPhotoRepository;

    public EventsController(
        IEventRepository eventRepository,
        IEventPhotoRepository eventPhotoRepository)
    {
        _eventRepository = eventRepository;
        _eventPhotoRepository = eventPhotoRepository;
    }

    /// <summary>
    /// Get all events ordered by creation date (newest first)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var events = await _eventRepository.GetAllAsync();
        var eventDtos = events.Select(EventMapper.ToEventDto).ToList();
        return Ok(eventDtos);
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(id);
        if (eventEntity == null)
        {
            return NotFound($"Event with ID {id} not found.");
        }

        var eventDto = EventMapper.ToEventDto(eventEntity);
        return Ok(eventDto);
    }

    /// <summary>
    /// Get events by committee ID
    /// </summary>
    [HttpGet("committee/{committeeId}")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetByCommittee(int committeeId)
    {
        var events = await _eventRepository.GetByCommitteeIdAsync(committeeId);
        var eventDtos = events.Select(EventMapper.ToEventDto).ToList();
        return Ok(eventDtos);
    }

    /// <summary>
    /// Get important events
    /// </summary>
    [HttpGet("important")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetImportantEvents()
    {
        var events = await _eventRepository.GetImportantEventsAsync();
        var eventDtos = events.Select(EventMapper.ToEventDto).ToList();
        return Ok(eventDtos);
    }

    /// <summary>
    /// Get upcoming events (starting within 1 week)
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcomingEvents()
    {
        var events = await _eventRepository.GetUpcomingEventsAsync();
        var eventDtos = events.Select(EventMapper.ToEventDto).ToList();
        return Ok(eventDtos);
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventDto createEventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var eventEntity = EventMapper.ToEvent(createEventDto);
        var createdEvent = await _eventRepository.CreateAsync(eventEntity);
        var eventDto = EventMapper.ToEventDto(createdEvent);

        return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, eventDto);
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto updateEventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var eventEntity = await _eventRepository.GetByIdAsync(id);
        if (eventEntity == null)
        {
            return NotFound($"Event with ID {id} not found.");
        }

        EventMapper.UpdateEventFromDto(eventEntity, updateEventDto);
        await _eventRepository.UpdateAsync(eventEntity);

        return NoContent();
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _eventRepository.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound($"Event with ID {id} not found.");
        }

        return NoContent();
    }

    /// <summary>
    /// Upload cover photo for an event
    /// </summary>
    [HttpPost("{eventId}/cover")]
    [Authorize]
    public async Task<ActionResult<EventPhotoDto>> UploadCoverPhoto(int eventId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        try
        {
            var eventPhoto = await _eventPhotoRepository.CreateCoverPhotoAsync(eventId, file);
            var eventPhotoDto = EventPhotoMapper.ToEventPhotoDto(eventPhoto);

            return Ok(eventPhotoDto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get cover photo for an event
    /// </summary>
    [HttpGet("{eventId}/cover")]
    public async Task<ActionResult<EventPhotoDto>> GetCoverPhoto(int eventId)
    {
        var coverPhoto = await _eventPhotoRepository.GetCoverPhotoByEventIdAsync(eventId);
        if (coverPhoto == null)
        {
            return NotFound($"Cover photo not found for event with ID {eventId}.");
        }

        var eventPhotoDto = EventPhotoMapper.ToEventPhotoDto(coverPhoto);
        return Ok(eventPhotoDto);
    }

    /// <summary>
    /// Upload event photos (multiple files)
    /// </summary>
    [HttpPost("{eventId}/photos")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EventPhotoDto>>> UploadEventPhotos(int eventId, [FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("At least one file is required.");
        }

        try
        {
            var eventPhotos = await _eventPhotoRepository.CreateEventPhotosAsync(eventId, files);
            var eventPhotoDtos = EventPhotoMapper.ToEventPhotoDtoList(eventPhotos).ToList();

            return Ok(eventPhotoDtos);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get event photos (non-cover photos)
    /// </summary>
    [HttpGet("{eventId}/photos")]
    public async Task<ActionResult<IEnumerable<EventPhotoDto>>> GetEventPhotos(int eventId)
    {
        var eventPhotos = await _eventPhotoRepository.GetEventPhotosByEventIdAsync(eventId);
        var eventPhotoDtos = EventPhotoMapper.ToEventPhotoDtoList(eventPhotos).ToList();

        return Ok(eventPhotoDtos);
    }

    /// <summary>
    /// Delete an event photo
    /// </summary>
    [HttpDelete("{eventId}/photos/{photoId}")]
    [Authorize]
    public async Task<IActionResult> DeleteEventPhoto(int eventId, int photoId)
    {
        var photo = await _eventPhotoRepository.GetByIdAsync(photoId);
        if (photo == null)
        {
            return NotFound($"Photo with ID {photoId} not found.");
        }

        if (photo.EventId != eventId)
        {
            return BadRequest("Photo does not belong to the specified event.");
        }

        var deleted = await _eventPhotoRepository.DeleteAsync(photoId);
        if (!deleted)
        {
            return NotFound($"Photo with ID {photoId} not found.");
        }

        return NoContent();
    }
}

