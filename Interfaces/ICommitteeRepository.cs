using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface ICommitteeRepository
{
    Task<List<Committee>> GetAllAsync();
    Task<Committee?> GetByIdAsync(int id);
    Task CreateAsync(Committee committee);
    Task UpdateAsync(Committee committee);
    Task DeleteAsync(Committee committee);
}