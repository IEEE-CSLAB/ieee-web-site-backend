namespace IEEEBackend.Models;

public class Committee : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    // Navigation properties
    public ICollection<Executive> Executives { get; set; } = new List<Executive>();
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    public ICollection<EventCommittee> EventCommittees { get; set; } = new List<EventCommittee>();
}

