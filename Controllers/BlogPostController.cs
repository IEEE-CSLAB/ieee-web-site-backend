using IEEEBackend.Dtos.BlogPost;
using IEEEBackend.Interfaces;
using IEEEBackend.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostController : ControllerBase
{
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IStorageService _storageService;
        private readonly IConfiguration _configuration;

        public BlogPostController(
            IBlogPostRepository blogPostRepository,
            IStorageService storageService,
            IConfiguration configuration)
        {
            _blogPostRepository = blogPostRepository;
            _storageService = storageService;
            _configuration = configuration;
        }

        // GET: api/blogposts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var blogPosts = await _blogPostRepository.GetAllAsync();
            var bpDto = blogPosts.Select(bp => bp.ToBlogPostDto());

            return Ok(bpDto);
        }
        // GET: api/blogposts/{id}

        // GET: api/blogposts/committee/{committeeId}
        [HttpGet("committee/{committeeId}")]
        public async Task<IActionResult> GetByCommittee([FromRoute] int committeeId)
        {
            var blogPosts = await _blogPostRepository.GetByCommitteeAsync(committeeId);

            var bpDto = blogPosts.Select(bp => bp.ToBlogPostDto());

            return Ok(bpDto);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var blogPost = await _blogPostRepository.GetByIdAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return Ok(blogPost.ToBlogPostDto());
        }

        [HttpGet("last8")]
        public async Task<IActionResult> GetLast8()
        {
            var blogPosts = await _blogPostRepository.GetLast8Async();
            var bpDto = blogPosts.Select(bp => bp.ToBlogPostDto());

            return Ok(bpDto);
        }

        // post: api/blogposts
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateBlogPostRequestDto createDto)
        {
            var blogPost = createDto.ToBlogPostFromCreateDto();
            var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
            return CreatedAtAction(nameof(GetById), new { id = createdBlogPost.Id }, createdBlogPost.ToBlogPostDto());
        }

        // put: api/blogposts/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateBlogPostRequestDto updateDto)
        {
            var updatedBlogPost = await _blogPostRepository.UpdateAsync(id, updateDto);
            if (updatedBlogPost == null)
            {
                return NotFound();
            }

            return Ok(updatedBlogPost.ToBlogPostDto());
        }

        // delete: api/blogposts/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var deletedBlogPost = await _blogPostRepository.DeleteAsync(id);
            if (deletedBlogPost == null)
            {
                return NotFound();
            }

            return Ok(deletedBlogPost.ToBlogPostDto());
        }

        /// <summary>
        /// Upload cover image for a blog post
        /// </summary>
        [HttpPost("{blogId}/cover")]
        [Authorize]
        public async Task<IActionResult> UploadCover(int blogId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            try
            {
                // Validate blog post exists
                var blogPost = await _blogPostRepository.GetByIdAsync(blogId);
                if (blogPost == null)
                {
                    return NotFound($"Blog post with ID {blogId} not found.");
                }

                // Validate file
                ValidateFile(file);

                // Upload to Supabase
                var folder = $"blogs/{blogId}/cover";
                using var stream = file.OpenReadStream();
                var filePath = await _storageService.UploadFileAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    folder
                );

                // Get public URL
                var coverImageUrl = _storageService.GetPublicUrl(filePath);

                // Update blog post cover image URL
                blogPost.CoverImageUrl = coverImageUrl;
                blogPost.UpdatedAt = DateTime.UtcNow;
                await _blogPostRepository.UpdateAsync(blogId, new UpdateBlogPostRequestDto
                {
                    CommitteeId = blogPost.CommitteeId,
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    CoverImageUrl = coverImageUrl
                });

                return Ok(new { coverImageUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private void ValidateFile(IFormFile file)
        {
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 5242880); // 5MB default
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions")
                .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            if (file.Length > maxFileSize)
            {
                throw new ArgumentException($"File size exceeds the maximum allowed size of {maxFileSize / 1024 / 1024}MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
            }
        }
    }