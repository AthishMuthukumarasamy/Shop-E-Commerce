using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public AdminApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: ALL PRODUCTS
        // =====================================================
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Retailer)
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
        }

        // =====================================================
        // GET: PRODUCT BY ID
        // =====================================================
        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Retailer)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        // =====================================================
        // ADD PRODUCT
        // =====================================================
        [HttpPost("product")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product created successfully",
                product.ProductId
            });
        }

        // =====================================================
        // UPDATE PRODUCT
        // =====================================================
        [HttpPut("product/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.ProductName = updatedProduct.ProductName;
            product.Price = updatedProduct.Price;
            product.CategoryId = updatedProduct.CategoryId;
            product.BrandId = updatedProduct.BrandId;
            product.RetailerId = updatedProduct.RetailerId;
            product.IsActive = updatedProduct.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully" });
        }

        // =====================================================
        // DELETE PRODUCT
        // =====================================================
        [HttpDelete("product/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        // =====================================================
        // GET CATEGORIES (for dropdown)
        // =====================================================
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // =====================================================
        // GET BRANDS (for dropdown)
        // =====================================================
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _context.Brands
                .AsNoTracking()
                .ToListAsync();

            return Ok(brands);
        }
    }
}
