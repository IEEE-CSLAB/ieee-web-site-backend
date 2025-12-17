using AutoMapper;
using IEEEBackend.Dtos.Executive;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutiveController : ControllerBase
{
    private readonly IExecutiveRepository _repository;
    private readonly IMapper _mapper;

    public ExecutiveController(IExecutiveRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all executives
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ExecutiveDto>>> GetAll()
    {
        var executives = await _repository.GetAllAsync();
        var dtos = _mapper.Map<List<ExecutiveDto>>(executives);
        return Ok(dtos);
    }

    /// <summary>
    /// Get executive by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExecutiveDto>> GetById(int id)
    {
        var executive = await _repository.GetByIdAsync(id);
        if (executive == null)
        {
            return NotFound(new { message = $"Executive with ID {id} not found." });
        }

        var dto = _mapper.Map<ExecutiveDto>(executive);
        return Ok(dto);
    }

    /// <summary>
    /// Get executives by committee ID
    /// </summary>
    [HttpGet("committee/{committeeId}")]
    public async Task<ActionResult<List<ExecutiveDto>>> GetByCommittee(int committeeId)
    {
        var executives = await _repository.GetByCommitteeIdAsync(committeeId);
        var dtos = _mapper.Map<List<ExecutiveDto>>(executives);
        return Ok(dtos);
    }

    /// <summary>
    /// Create a new executive
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ExecutiveDto>> Create([FromBody] CreateExecutiveDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var executive = _mapper.Map<Executive>(createDto);
        var createdExecutive = await _repository.CreateAsync(executive);
        var dto = _mapper.Map<ExecutiveDto>(createdExecutive);

        return CreatedAtAction(nameof(GetById), new { id = createdExecutive.Id }, dto);
    }

    /// <summary>
    /// Update an existing executive
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExecutiveDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingExecutive = await _repository.GetByIdAsync(id);
        if (existingExecutive == null)
        {
            return NotFound(new { message = $"Executive with ID {id} not found." });
        }

        _mapper.Map(updateDto, existingExecutive);
        await _repository.UpdateAsync(existingExecutive);

        return NoContent();
    }

    /// <summary>
    /// Delete an executive
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Executive with ID {id} not found." });
        }

        return NoContent();
    }
}

