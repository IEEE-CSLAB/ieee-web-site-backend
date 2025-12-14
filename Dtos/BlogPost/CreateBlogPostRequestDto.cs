using System.ComponentModel.DataAnnotations;

namespace IEEEBackend.Dtos.BlogPost;

public class CreateBlogPostRequestDto
{
    [Required]
    public int CommitteeId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? CoverImageUrl { get; set; }
}