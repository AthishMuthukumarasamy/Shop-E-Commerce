using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public PaymentApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET PAYMENT PAGE (ORDER SUMMARY)
        // =====================================================
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderForPayment(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        // =====================================================
        // PROCESS PAYMENT
        // =====================================================
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment(int orderId, string paymentMethod)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            // Generate transaction ID
            string transactionId = Guid.NewGuid().ToString();

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

            // Update order status
            order.OrderStatus = "Confirmed";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment successful",
                paymentId = payment.PaymentId,
                transactionId
            });
        }

        // =====================================================
        // PAYMENT SUCCESS DETAILS
        // =====================================================
        [HttpGet("success/{paymentId}")]
        public async Task<IActionResult> PaymentSuccess(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            return Ok(payment);
        }

        // =====================================================
        // USER PAYMENT HISTORY
        // =====================================================
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPayments(int userId)
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return Ok(payments);
        }

        // =====================================================
        // ALL PAYMENTS (ADMIN)
        // =====================================================
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return Ok(payments);
        }
    }
}