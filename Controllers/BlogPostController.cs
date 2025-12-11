using IEEEBackend.Dtos.BlogPost;
using IEEEBackend.Interfaces;
using IEEEBackend.Mappers;
using IEEEBackend.Repository;
using Microsoft.AspNetCore.Mvc;
//Posts: getall(orderbytime) – getbyid - GetByCommittee -getlast8-create*-update*
namespace IEEEBackend.Controllers
{
    [Route("api/blogposts")]
    [ApiController]
    public class BlogPostController : ControllerBase
    {
        private readonly IBlogPostRepository _blogPostRepository;

        public BlogPostController(IBlogPostRepository blogPostRepository)
        {
            _blogPostRepository = blogPostRepository;
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

        // GET: api/committee/{committeeId}/blogposts/ 
           
        [HttpGet("/committee/{committeeId}/blogposts")]
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
        public async Task<IActionResult> Create([FromBody] CreateBlogPostRequestDto createDto)
        {
            var blogPost = createDto.ToBlogPostFromCreateDto();
            var createdBlogPost = await _blogPostRepository.CreateAsync(blogPost);
            return CreatedAtAction(nameof(GetById), new { id = createdBlogPost.Id }, createdBlogPost.ToBlogPostDto());
        }

        // put: api/blogposts/{id}
        [HttpPut("{id}")]
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
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var deletedBlogPost = await _blogPostRepository.DeleteAsync(id);
            if (deletedBlogPost == null)
            {
                return NotFound();
            }

            return Ok(deletedBlogPost.ToBlogPostDto());
        }


    }
}