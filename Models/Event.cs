namespace IEEEBackend.Models;

public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public int? Quota { get; set; }
    public bool IsImportant { get; set; }

    // Navigation property
    public ICollection<EventCommittee> EventCommittees { get; set; } = new List<EventCommittee>();
}

