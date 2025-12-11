namespace IEEEBackend.Dtos.BlogPost
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        public int CommitteeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}