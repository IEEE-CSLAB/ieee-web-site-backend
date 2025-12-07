using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IEEEBackend.Data;
using IEEEBackend.DTOs;
using IEEEBackend.Models;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class CommitteeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CommitteeController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Committees
                                     .OrderByDescending(x => x.CreatedAt)
                                     .ToListAsync();

        var dtos = _mapper.Map<List<CommitteeDto>>(entities);

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _context.Committees.FindAsync(id);

        if (entity == null)
            return NotFound(new { message = $"Committee with ID {id} not found." });

        var dto = _mapper.Map<CommitteeDto>(entity);
        return Ok(dto);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CommitteeCreateDto input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<Committee>(input);

        await _context.Committees.AddAsync(entity);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Committee created successfully.", id = entity.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CommitteeCreateDto input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingEntity = await _context.Committees.FindAsync(id);

        if (existingEntity == null)
            return NotFound(new { message = $"Committee with ID {id} not found." });

        _mapper.Map(input, existingEntity);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Committees.FindAsync(id);

        if (entity == null)
            return NotFound(new { message = $"Committee with ID {id} not found." });

        _context.Committees.Remove(entity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Committee deleted successfully." });
    }
}