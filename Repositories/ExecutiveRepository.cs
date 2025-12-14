using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class ExecutiveRepository : IExecutiveRepository
{
    private readonly ApplicationDbContext _context;

    public ExecutiveRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Executive>> GetAllAsync()
    {
        return await _context.Executives
            .Include(e => e.Committee)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Executive?> GetByIdAsync(int id)
    {
        return await _context.Executives
            .Include(e => e.Committee)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Executive>> GetByCommitteeIdAsync(int committeeId)
    {
        return await _context.Executives
            .Include(e => e.Committee)
            .Where(e => e.CommitteeId == committeeId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Executive> CreateAsync(Executive executive)
    {
        executive.CreatedAt = DateTime.UtcNow;
        executive.UpdatedAt = DateTime.UtcNow;

        _context.Executives.Add(executive);
        await _context.SaveChangesAsync();

        // Reload with related entities
        return await GetByIdAsync(executive.Id) ?? executive;
    }

    public async Task<Executive> UpdateAsync(Executive executive)
    {
        executive.UpdatedAt = DateTime.UtcNow;

        var existingExecutive = await _context.Executives.FindAsync(executive.Id);
        if (existingExecutive == null)
        {
            throw new InvalidOperationException($"Executive with ID {executive.Id} not found.");
        }

        existingExecutive.CommitteeId = executive.CommitteeId;
        existingExecutive.FirstName = executive.FirstName;
        existingExecutive.LastName = executive.LastName;
        existingExecutive.Role = executive.Role;
        existingExecutive.ImageUrl = executive.ImageUrl;
        existingExecutive.UpdatedAt = executive.UpdatedAt;

        await _context.SaveChangesAsync();

        // Reload with related entities
        return await GetByIdAsync(existingExecutive.Id) ?? existingExecutive;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var executive = await _context.Executives.FindAsync(id);
        if (executive == null)
        {
            return false;
        }

        _context.Executives.Remove(executive);
        await _context.SaveChangesAsync();
        return true;
    }
}

