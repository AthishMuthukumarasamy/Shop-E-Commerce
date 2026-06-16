using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public WishlistApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET WISHLIST
        // =====================================================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWishlist(int userId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductImages)
                .Where(w => w.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            return Ok(wishlist);
        }

        // =====================================================
        // ADD TO WISHLIST
        // =====================================================
        [HttpPost("add")]
        public async Task<IActionResult> AddToWishlist(int userId, int productId)
        {
            var exists = await _context.Wishlists
                .FirstOrDefaultAsync(w =>
                    w.UserId == userId &&
                    w.ProductId == productId);

            if (exists != null)
                return Ok(new { message = "Already in wishlist" });

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                AddedDate = DateTime.Now
            };

            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to wishlist successfully" });
        }

        // =====================================================
        // REMOVE FROM WISHLIST
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.WishlistId == id);

            if (item == null)
                return NotFound(new { message = "Item not found" });

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from wishlist" });
        }

        // =====================================================
        // REMOVE BY USER + PRODUCT (OPTIONAL CLEAN METHOD)
        // =====================================================
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveByProduct(int userId, int productId)
        {
            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w =>
                    w.UserId == userId &&
                    w.ProductId == productId);

            if (item == null)
                return NotFound(new { message = "Item not found" });

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from wishlist" });
        }
    }
}
