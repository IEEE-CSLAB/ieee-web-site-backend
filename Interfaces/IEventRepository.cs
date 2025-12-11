using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task<IEnumerable<Event>> GetByCommitteeIdAsync(int committeeId);
    Task<IEnumerable<Event>> GetImportantEventsAsync();
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    Task<Event> CreateAsync(Event eventEntity);
    Task<Event> UpdateAsync(Event eventEntity);
    Task<bool> DeleteAsync(int id);
}

