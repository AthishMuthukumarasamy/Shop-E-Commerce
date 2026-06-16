using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RetailerApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public RetailerApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DASHBOARD STATS
        // =====================================================
        [HttpGet("dashboard/{userId}")]
        public async Task<IActionResult> Dashboard(int userId)
        {
            var totalProducts = await _context.Products
                .CountAsync(p => p.RetailerId == userId);

            var pendingProducts = await _context.Products
                .CountAsync(p => p.RetailerId == userId && p.Status == "Pending");

            var approvedProducts = await _context.Products
                .CountAsync(p => p.RetailerId == userId && p.Status == "Approved");

            return Ok(new
            {
                totalProducts,
                pendingProducts,
                approvedProducts
            });
        }

        // =====================================================
        // MY PRODUCTS
        // =====================================================
        [HttpGet("products/{userId}")]
        public async Task<IActionResult> GetMyProducts(int userId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.RetailerId == userId)
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
        }

        // =====================================================
        // CREATE PRODUCT
        // =====================================================
        [HttpPost("product")]
        public async Task<IActionResult> CreateProduct(int userId, [FromBody] Product product)
        {
            product.RetailerId = userId;
            product.CreatedDate = DateTime.Now;
            product.Status = "Pending";

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product created successfully",
                product.ProductId
            });
        }

        // =====================================================
        // GET PRODUCT DETAILS
        // =====================================================
        [HttpGet("product/{userId}/{id}")]
        public async Task<IActionResult> GetProduct(int userId, int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.RetailerId == userId);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        // =====================================================
        // UPDATE PRODUCT
        // =====================================================
        [HttpPut("product/{userId}/{id}")]
        public async Task<IActionResult> UpdateProduct(int userId, int id, [FromBody] Product updated)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.RetailerId == userId);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.ProductName = updated.ProductName;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Stock = updated.Stock;
            product.CategoryId = updated.CategoryId;
            product.BrandId = updated.BrandId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully" });
        }

        // =====================================================
        // DELETE PRODUCT
        // =====================================================
        [HttpDelete("product/{userId}/{id}")]
        public async Task<IActionResult> DeleteProduct(int userId, int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.RetailerId == userId);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        // =====================================================
        // DROPDOWNS (CATEGORIES)
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
        // DROPDOWNS (BRANDS)
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
