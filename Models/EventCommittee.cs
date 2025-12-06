namespace IEEEBackend.Models;

public class EventCommittee
{
    public int EventId { get; set; }
    public int CommitteeId { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Committee Committee { get; set; } = null!;
}

