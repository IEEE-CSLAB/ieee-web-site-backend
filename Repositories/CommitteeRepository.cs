using Microsoft.EntityFrameworkCore;
using IEEEBackend.Data;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;

namespace IEEEBackend.Repositories;

public class CommitteeRepository : ICommitteeRepository
{
    private readonly ApplicationDbContext _context;

    public CommitteeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Committee>> GetAllAsync()
    {
        return await _context.Committees
                             .OrderByDescending(x => x.CreatedAt)
                             .ToListAsync();
    }

    public async Task<Committee?> GetByIdAsync(int id)
    {
        return await _context.Committees.FindAsync(id);
    }

    public async Task CreateAsync(Committee committee)
    {
        await _context.Committees.AddAsync(committee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Committee committee)
    {
        _context.Committees.Update(committee);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Committee committee)
    {
        _context.Committees.Remove(committee);
        await _context.SaveChangesAsync();
    }
}