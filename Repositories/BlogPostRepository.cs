using IEEEBackend.Data;
using IEEEBackend.Dtos.BlogPost;
using IEEEBackend.Interfaces;
using IEEEBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IEEEBackend.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly ApplicationDbContext _context;
    public BlogPostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPost> CreateAsync(BlogPost blogPost)
    {
        await _context.BlogPosts.AddAsync(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<BlogPost?> UpdateAsync(int id, UpdateBlogPostRequestDto updateDto)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return null;
        }

        blogPost.CommitteeId = updateDto.CommitteeId;
        blogPost.Title = updateDto.Title;
        blogPost.Content = updateDto.Content;
        blogPost.CoverImageUrl = updateDto.CoverImageUrl;
        blogPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<BlogPost?> DeleteAsync(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return null;
        }

        _context.BlogPosts.Remove(blogPost);
        await _context.SaveChangesAsync();
        return blogPost;
    }

    public async Task<List<BlogPost>> GetAllAsync()
    {
        return await _context.BlogPosts
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BlogPost>> GetByCommitteeAsync(int committeeId)
    {
        return await _context.BlogPosts
            .Where(bp => bp.CommitteeId == committeeId)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();
    }

    public async Task<BlogPost?> GetByIdAsync(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);

        if (blogPost == null)
        {
            return null;
        }
        return blogPost;
    }

    public async Task<List<BlogPost>> GetLast8Async()
    {
        return await _context.BlogPosts
            .OrderByDescending(bp => bp.CreatedAt)
            .Take(8)
            .ToListAsync();
    }
}