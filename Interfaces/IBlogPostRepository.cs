using IEEEBackend.Dtos.BlogPost;
using IEEEBackend.Models;

namespace IEEEBackend.Interfaces;

public interface IBlogPostRepository
{
    Task<List<BlogPost>> GetAllAsync();
    Task<BlogPost?> GetByIdAsync(int id);
    Task<List<BlogPost>> GetByCommitteeAsync(int committeeId);
    Task<List<BlogPost>> GetLast8Async();
    Task<BlogPost?> DeleteAsync(int id);
    Task<BlogPost?> UpdateAsync(int id, UpdateBlogPostRequestDto updateDto);
    Task<BlogPost> CreateAsync(BlogPost blogPost);
}