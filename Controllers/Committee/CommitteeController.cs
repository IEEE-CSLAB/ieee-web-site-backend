using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using IEEEBackend.Dtos;
using IEEEBackend.Models;
using IEEEBackend.Interfaces;
using IEEEBackend.Services;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommitteeController : ControllerBase
{
    private readonly ICommitteeRepository _repository;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public CommitteeController(ICommitteeRepository repository, IMapper mapper, IFileStorageService fileStorage)
    {
        _repository = repository;
        _mapper = mapper;
        _fileStorage = fileStorage;
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

    /// <summary>
    /// Upload logo for a committee
    /// </summary>
    [HttpPost("{id}/logo")]
    [Authorize]
    public async Task<ActionResult<CommitteeDto>> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var committee = await _repository.GetByIdAsync(id);
        if (committee == null)
        {
            return NotFound(new { message = $"Committee with ID {id} not found." });
        }

        try
        {
            // Delete old logo if exists
            if (!string.IsNullOrEmpty(committee.LogoUrl))
            {
                await _fileStorage.DeleteFileAsync(committee.LogoUrl);
            }

            // Save new logo
            var container = $"committees/{id}/logo";
            var relativePath = await _fileStorage.SaveFileAsync(container, file);

            // Update committee
            committee.LogoUrl = relativePath;
            await _repository.UpdateAsync(committee);

            var dto = _mapper.Map<CommitteeDto>(committee);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}