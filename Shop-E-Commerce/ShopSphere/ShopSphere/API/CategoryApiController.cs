using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public CategoryApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL CATEGORIES
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================================
        // GET CATEGORY BY ID (optional but useful)
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }

        // =====================================================
        // CREATE CATEGORY
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category created successfully",
                category.CategoryId
            });
        }

        // =====================================================
        // DELETE CATEGORY
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}