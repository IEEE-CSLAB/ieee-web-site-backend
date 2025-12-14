using IEEEBackend.Dtos.BlogPost;
using IEEEBackend.Models;

namespace IEEEBackend.Mappers;

public static class BlogPostMappers
{
        public static BlogPostDto ToBlogPostDto(this BlogPost blogPost)
        {
            return new BlogPostDto
            {
                Id = blogPost.Id,
                CommitteeId = blogPost.CommitteeId,
                Title = blogPost.Title,
                Content = blogPost.Content,
                CoverImageUrl = blogPost.CoverImageUrl,
                CreatedAt = blogPost.CreatedAt,
                UpdatedAt = blogPost.UpdatedAt
            };
        }
        public static BlogPost ToBlogPostFromCreateDto(this CreateBlogPostRequestDto createDto)
        {
            return new BlogPost
            {
                CommitteeId = createDto.CommitteeId,
                Title = createDto.Title,
                Content = createDto.Content,
                CoverImageUrl = createDto.CoverImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }