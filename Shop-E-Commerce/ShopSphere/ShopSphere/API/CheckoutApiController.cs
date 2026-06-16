using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public CheckoutApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET CHECKOUT DATA (CART SUMMARY)
        // =====================================================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCheckout(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest(new { message = "Cart is empty" });

            var total = cart.CartItems.Sum(x => x.Quantity * x.UnitPrice);

            return Ok(new
            {
                cart,
                totalAmount = total
            });
        }

        // =====================================================
        // PLACE ORDER
        // =====================================================
        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest(new { message = "Cart is empty" });

            // Calculate total
            decimal total = cart.CartItems.Sum(x => x.Quantity * x.UnitPrice);

            // Create Order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                OrderStatus = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create Order Details
            var orderDetails = cart.CartItems.Select(item => new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.UnitPrice
            }).ToList();

            _context.OrderDetails.AddRange(orderDetails);

            // Clear Cart
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order placed successfully",
                orderId = order.OrderId,
                totalAmount = total
            });
        }
    }
}