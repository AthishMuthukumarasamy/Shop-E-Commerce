using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public CartApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET CART
        // =====================================================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return Ok(new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                });
            }

            return Ok(cart);
        }

        // =====================================================
        // ADD TO CART
        // =====================================================
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int userId, int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.CartId &&
                    ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = 1,
                    UnitPrice = product.Price
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to cart successfully" });
        }

        // =====================================================
        // REMOVE ITEM
        // =====================================================
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(x => x.CartItemId == cartItemId);

            if (item == null)
                return NotFound(new { message = "Item not found" });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed successfully" });
        }

        // =====================================================
        // INCREASE QUANTITY
        // =====================================================
        [HttpPut("increase/{cartItemId}")]
        public async Task<IActionResult> Increase(int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(x => x.CartItemId == cartItemId);

            if (item == null)
                return NotFound(new { message = "Item not found" });

            item.Quantity++;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Quantity increased" });
        }

        // =====================================================
        // DECREASE QUANTITY
        // =====================================================
        [HttpPut("decrease/{cartItemId}")]
        public async Task<IActionResult> Decrease(int cartItemId)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(x => x.CartItemId == cartItemId);

            if (item == null)
                return NotFound(new { message = "Item not found" });

            item.Quantity--;

            if (item.Quantity <= 0)
                _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Quantity updated" });
        }
    }
}
