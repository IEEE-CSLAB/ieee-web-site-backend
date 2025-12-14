using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Admin?> GetByUsernameAsync(string username)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    public async Task<Admin?> GetByIdAsync(int id)
    {
        return await _context.Admins.FindAsync(id);
    }

    public async Task<bool> UpdatePasswordAsync(int adminId, string newPasswordHash)
    {
        var admin = await _context.Admins.FindAsync(adminId);
        if (admin == null)
        {
            return false;
        }

        admin.PasswordHash = newPasswordHash;
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}

