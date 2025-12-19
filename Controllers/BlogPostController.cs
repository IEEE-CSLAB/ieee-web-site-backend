using IEEEBackend.Dtos.BlogPost;
using IEEEBackend.Interfaces;
using IEEEBackend.Mappers;
using IEEEBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IEEEBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostController : ControllerBase
{
        private readonly IBlogPostRepository _blogPostRepository;
        private readonly IFileStorageService _fileStorage;

        public BlogPostController(IBlogPostRepository blogPostRepository, IFileStorageService fileStorage)
        {
            _blogPostRepository = blogPostRepository;
            _fileStorage = fileStorage;
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
        /// Upload cover photo for a blog post
        /// </summary>
        [HttpPost("{id}/cover")]
        [Authorize]
        public async Task<ActionResult<BlogPostDto>> UploadCover(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var blogPost = await _blogPostRepository.GetByIdAsync(id);
            if (blogPost == null)
            {
                return NotFound($"Blog post with ID {id} not found.");
            }

            try
            {
                // Delete old cover if exists
                if (!string.IsNullOrEmpty(blogPost.CoverImageUrl))
                {
                    await _fileStorage.DeleteFileAsync(blogPost.CoverImageUrl);
                }

                // Save new cover
                var container = $"blogs/{id}/cover";
                var relativePath = await _fileStorage.SaveFileAsync(container, file);

                // Update blog post with new cover URL
                var updatedBlogPost = await _blogPostRepository.UpdateAsync(id, new UpdateBlogPostRequestDto
                {
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    CommitteeId = blogPost.CommitteeId,
                    CoverImageUrl = relativePath
                });

                return Ok(updatedBlogPost!.ToBlogPostDto());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }