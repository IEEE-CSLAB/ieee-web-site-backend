namespace IEEEBackend.Models;

public class BlogPost : BaseEntity
{
    public int CommitteeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }

    // Navigation property
    public Committee Committee { get; set; } = null!;
}

