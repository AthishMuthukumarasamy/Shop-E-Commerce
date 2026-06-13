using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ShoppDbContext _context;

        public CheckoutController(ShoppDbContext context)
        {
            _context = context;
        }

       
        // CHECKOUT PAGE
        
        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            return View(cart);
        }

       
        // PLACE ORDER
        
        public IActionResult PlaceOrder()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

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
            _context.SaveChanges();

            // Create Order Details
            foreach (var item in cart.CartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice
                });
            }

            // Clear Cart
            _context.CartItems.RemoveRange(cart.CartItems);

            _context.SaveChanges();

            return RedirectToAction("Pay", "Payment", new { orderId = order.OrderId });
        }
    }
}