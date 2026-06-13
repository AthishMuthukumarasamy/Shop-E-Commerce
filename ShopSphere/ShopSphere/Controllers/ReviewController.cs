using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ShoppDbContext _context;

        public ReviewController(ShoppDbContext context)
        {
            _context = context;
        }

        
        // ADD REVIEW
       
        [HttpPost]
        public IActionResult Add(int productId, int rating, string reviewText)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                ReviewText = reviewText,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        
        // DELETE REVIEW
      
        public IActionResult Delete(int id)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == id);

            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Products");
        }
    }
}
