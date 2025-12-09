
namespace IEEEBackend.Dtos.BlogPost
{
    public class UpdateBlogPostRequestDto
    {

        public int CommitteeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? CoverImageUrl { get; set; }
        
    }
}