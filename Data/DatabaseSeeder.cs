using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAdminAsync()
    {
        // Admin zaten var mı kontrol et
        var existingAdmin = await _context.Admins.FirstOrDefaultAsync();
        if (existingAdmin != null)
        {
            return; // Admin zaten var
        }

        // Default admin oluştur
        var admin = new Admin
        {
            Username = "admin",
            PasswordHash = _passwordHasher.HashPassword("admin123"), // Default password
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
    }
}

