namespace IEEEBackend.Models;

public class Executive : BaseEntity
{
    public int CommitteeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    // Navigation property
    public Committee Committee { get; set; } = null!;
}

