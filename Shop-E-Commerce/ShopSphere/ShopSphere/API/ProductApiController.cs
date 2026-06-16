using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public ProductApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 1. GET PRODUCTS (SEARCH + FILTER + SORT)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            string? searchString,
            string? sortOrder,
            int? categoryId,
            int? brandId)
        {
            var products = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    EF.Functions.Like(p.ProductName, $"%{searchString}%") ||
                    EF.Functions.Like(p.Description, $"%{searchString}%"));
            }

            // FILTER CATEGORY
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            // FILTER BRAND
            if (brandId.HasValue)
                products = products.Where(p => p.BrandId == brandId);

            // SORTING
            products = sortOrder switch
            {
                "price_low" => products.OrderBy(p => p.Price),
                "price_high" => products.OrderByDescending(p => p.Price),
                "name" => products.OrderBy(p => p.ProductName),
                _ => products.OrderByDescending(p => p.ProductId)
            };

            var result = await products.AsNoTracking().ToListAsync();

            return Ok(result);
        }

        // =====================================================
        // 2. GET PRODUCT DETAILS
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        // =====================================================
        // 3. CREATE PRODUCT
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
        // 4. UPDATE PRODUCT
        // =====================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.ProductName = updatedProduct.ProductName;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.CategoryId = updatedProduct.CategoryId;
            product.BrandId = updatedProduct.BrandId;
            product.Status = updatedProduct.Status;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully" });
        }

        // =====================================================
        // 5. DELETE PRODUCT
        // =====================================================
        [HttpDelete("{id}")]
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
        // 6. GET DROPDOWN DATA (CATEGORIES)
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
        // 7. GET DROPDOWN DATA (BRANDS)
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