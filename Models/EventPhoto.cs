namespace IEEEBackend.Models;

public class EventPhoto : BaseEntity
{
    public int EventId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsCover { get; set; }

    // Navigation property
    public Event Event { get; set; } = null!;
}

