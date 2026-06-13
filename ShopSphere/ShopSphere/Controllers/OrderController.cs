using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class OrderController : Controller
    {
        private readonly ShoppDbContext _context;

        public OrderController(ShoppDbContext context)
        {
            _context = context;
        }

        // PLACE ORDER (CHECKOUT)
       
        public IActionResult PlaceOrder()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Get user cart with products
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            // Calculate total amount
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
            _context.SaveChanges();

            // Create Order Details
            foreach (var item in cart.CartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice
                };

                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges();

            // Clear Cart after order
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.SaveChanges();

            return RedirectToAction("OrderSuccess", new { id = order.OrderId });
        }

        // ORDER SUCCESS PAGE
      
        public IActionResult OrderSuccess(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // ORDER HISTORY (MY ORDERS)
       
        public IActionResult MyOrders()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // ORDER DETAILS
        
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}