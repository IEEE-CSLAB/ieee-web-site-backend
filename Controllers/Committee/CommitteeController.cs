using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using IEEEBackend.Dtos;
using IEEEBackend.Models;
using IEEEBackend.Interfaces;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommitteeController : ControllerBase
{
    private readonly ICommitteeRepository _repository;
    private readonly IMapper _mapper;

    public CommitteeController(ICommitteeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _repository.GetAllAsync();

        var dtos = _mapper.Map<List<CommitteeDto>>(entities);

        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);

        if (entity == null)
            return NotFound(new { message = $"Committee with ID {id} not found." });

        var dto = _mapper.Map<CommitteeDto>(entity);
        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CommitteeCreateDto input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = _mapper.Map<Committee>(input);

        await _repository.CreateAsync(entity);

        return Ok(new { message = "Committee created successfully.", id = entity.Id });
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] CommitteeCreateDto input)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existingEntity = await _repository.GetByIdAsync(id);

        if (existingEntity == null)
            return NotFound(new { message = $"Committee with ID {id} not found." });

        _mapper.Map(input, existingEntity);

        await _repository.UpdateAsync(existingEntity);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);

        if (entity == null)
            return NotFound(new { message = $"Committee with ID {id} not found." });

        await _repository.DeleteAsync(entity);

        return Ok(new { message = "Committee deleted successfully." });
    }
}