using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface IExecutiveRepository
{
    Task<List<Executive>> GetAllAsync();
    Task<Executive?> GetByIdAsync(int id);
    Task<List<Executive>> GetByCommitteeIdAsync(int committeeId);
    Task<Executive> CreateAsync(Executive executive);
    Task<Executive> UpdateAsync(Executive executive);
    Task<bool> DeleteAsync(int id);
}

