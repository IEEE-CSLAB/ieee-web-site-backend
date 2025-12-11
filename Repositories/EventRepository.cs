using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.EventCommittees)
                .ThenInclude(ec => ec.Committee)
            .Include(e => e.EventPhotos)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.EventCommittees)
                .ThenInclude(ec => ec.Committee)
            .Include(e => e.EventPhotos)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetByCommitteeIdAsync(int committeeId)
    {
        return await _context.Events
            .Include(e => e.EventCommittees)
                .ThenInclude(ec => ec.Committee)
            .Include(e => e.EventPhotos)
            .Where(e => e.EventCommittees.Any(ec => ec.CommitteeId == committeeId))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetImportantEventsAsync()
    {
        return await _context.Events
            .Include(e => e.EventCommittees)
                .ThenInclude(ec => ec.Committee)
            .Include(e => e.EventPhotos)
            .Where(e => e.IsImportant)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
    {
        var now = DateTime.UtcNow;
        var oneWeekLater = now.AddDays(7);

        return await _context.Events
            .Include(e => e.EventCommittees)
                .ThenInclude(ec => ec.Committee)
            .Include(e => e.EventPhotos)
            .Where(e => e.StartDate >= now && e.StartDate <= oneWeekLater)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<Event> CreateAsync(Event eventEntity)
    {
        eventEntity.CreatedAt = DateTime.UtcNow;
        eventEntity.UpdatedAt = DateTime.UtcNow;

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();
        return eventEntity;
    }

    public async Task<Event> UpdateAsync(Event eventEntity)
    {
        eventEntity.UpdatedAt = DateTime.UtcNow;

        // Load existing event with EventCommittees for proper tracking
        var existingEvent = await _context.Events
            .Include(e => e.EventCommittees)
            .FirstOrDefaultAsync(e => e.Id == eventEntity.Id);

        if (existingEvent == null)
        {
            throw new InvalidOperationException($"Event with ID {eventEntity.Id} not found.");
        }

        // Update event properties
        existingEvent.Title = eventEntity.Title;
        existingEvent.Description = eventEntity.Description;
        existingEvent.StartDate = eventEntity.StartDate;
        existingEvent.EndDate = eventEntity.EndDate;
        existingEvent.Location = eventEntity.Location;
        existingEvent.Quota = eventEntity.Quota;
        existingEvent.IsImportant = eventEntity.IsImportant;
        existingEvent.UpdatedAt = eventEntity.UpdatedAt;

        // Update EventCommittees
        // Remove existing EventCommittees
        _context.EventCommittees.RemoveRange(existingEvent.EventCommittees);

        // Add new EventCommittees
        if (eventEntity.EventCommittees != null && eventEntity.EventCommittees.Any())
        {
            foreach (var eventCommittee in eventEntity.EventCommittees)
            {
                eventCommittee.EventId = existingEvent.Id;
                _context.EventCommittees.Add(eventCommittee);
            }
        }

        await _context.SaveChangesAsync();
        
        // Reload with committees for return
        return await GetByIdAsync(existingEvent.Id) ?? existingEvent;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return false;
        }

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
        return true;
    }
}

