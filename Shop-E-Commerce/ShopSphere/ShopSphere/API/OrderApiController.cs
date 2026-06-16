using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public OrderApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // PLACE ORDER (CHECKOUT)
        // =====================================================
        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest(new { message = "Cart is empty" });

            // Calculate total
            decimal totalAmount = cart.CartItems
                .Sum(x => x.Quantity * x.UnitPrice);

            // Create Order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
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
                totalAmount
            });
        }

        // =====================================================
        // GET ORDER SUCCESS / DETAILS
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        // =====================================================
        // GET USER ORDERS (MY ORDERS)
        // =====================================================
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // =====================================================
        // GET ALL ORDERS (ADMIN USE)
        // =====================================================
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }
    }
}
