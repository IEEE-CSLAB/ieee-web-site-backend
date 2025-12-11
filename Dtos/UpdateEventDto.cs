using System.ComponentModel.DataAnnotations;

namespace IEEEBackend.Dtos;

public class UpdateEventDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public string? Location { get; set; }

    public int? Quota { get; set; }

    public bool IsImportant { get; set; }

    public List<int>? CommitteeIds { get; set; }
}

