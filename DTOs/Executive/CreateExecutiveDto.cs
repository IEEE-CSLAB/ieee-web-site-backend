using System.ComponentModel.DataAnnotations;

namespace IEEEBackend.Dtos.Executive;

public class CreateExecutiveDto
{
    [Required]
    public int CommitteeId { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
}

