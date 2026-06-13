using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ShoppDbContext _context;

        // Constructor - Injecting Database Context
        public PaymentController(ShoppDbContext context)
        {
            _context = context;
        }

        
        // 1. PAYMENT PAGE (SHOW ORDER SUMMARY)
        
        public IActionResult Pay(int orderId)
        {
            // Load order from DB
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderId == orderId);

            // If order not found
            if (order == null)
                return NotFound();

            return View(order);
        }

       
        // 2. PROCESS PAYMENT (SAVE PAYMENT DATA)
        
        [HttpPost]
        public IActionResult ProcessPayment(int orderId, string paymentMethod)
        {
            // Get order
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            // Generate unique transaction id
            string transactionId = Guid.NewGuid().ToString();

            // Create Payment record
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = order.TotalAmount,
                PaymentDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Success",
                TransactionId = transactionId
            };

            _context.Payments.Add(payment);

            // Update order status after payment
            order.OrderStatus = "Paid";

            _context.SaveChanges();

            return RedirectToAction("PaymentSuccess", new { id = payment.PaymentId });
        }

        // 3. PAYMENT SUCCESS PAGE
     
        public IActionResult PaymentSuccess(int id)
        {
            var payment = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        
        // 4. USER PAYMENT HISTORY
       
        public IActionResult MyPayments()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var payments = _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            return View(payments);
        }
    }
}