using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface IAdminRepository
{
    Task<Admin?> GetByUsernameAsync(string username);
    Task<Admin?> GetByIdAsync(int id);
    Task<bool> UpdatePasswordAsync(int adminId, string newPasswordHash);
}

