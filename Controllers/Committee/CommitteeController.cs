using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using IEEEBackend.Dtos;
using IEEEBackend.Models;
using IEEEBackend.Interfaces;
using System.IO;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommitteeController : ControllerBase
{
    private readonly ICommitteeRepository _repository;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;
    private readonly IConfiguration _configuration;

    public CommitteeController(
        ICommitteeRepository repository, 
        IMapper mapper,
        IStorageService storageService,
        IConfiguration configuration)
    {
        _repository = repository;
        _mapper = mapper;
        _storageService = storageService;
        _configuration = configuration;
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
    [HttpPost("{committeeId}/logo")]
    [Authorize]
    public async Task<IActionResult> UploadLogo(int committeeId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        try
        {
            // Validate committee exists
            var committee = await _repository.GetByIdAsync(committeeId);
            if (committee == null)
            {
                return NotFound($"Committee with ID {committeeId} not found.");
            }

            // Validate file
            ValidateFile(file);

            // Upload to Supabase
            var folder = $"committees/{committeeId}";
            using var stream = file.OpenReadStream();
            var filePath = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder
            );

            // Get public URL
            var logoUrl = _storageService.GetPublicUrl(filePath);

            // Update committee logo URL
            committee.LogoUrl = logoUrl;
            await _repository.UpdateAsync(committee);

            return Ok(new { logoUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private void ValidateFile(IFormFile file)
    {
        var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880); // 5MB default
        var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions")
            .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.");
        }

        if (file.Length > maxFileSize)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {maxFileSize / 1024 / 1024}MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
        }
    }
}