namespace IEEEBackend.Dtos.Executive;

public class ExecutiveDto
{
    public int Id { get; set; }
    public int CommitteeId { get; set; }
    public string CommitteeName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

