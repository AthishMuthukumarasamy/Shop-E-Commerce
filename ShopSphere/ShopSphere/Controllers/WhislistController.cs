using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ShoppDbContext _context;

        // Constructor - Inject DbContext
        public WishlistController(ShoppDbContext context)
        {
            _context = context;
        }

        // 1. VIEW WISHLIST
      
        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var wishlist = _context.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToList();

            return View(wishlist);
        }

       
        // 2. ADD TO WISHLIST
       
        public IActionResult Add(int productId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // check if already exists
            var exists = _context.Wishlists
                .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (exists == null)
            {
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.Now
                };

                _context.Wishlists.Add(wishlistItem);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // 3. REMOVE FROM WISHLIST
       
        public IActionResult Remove(int id)
        {
            var item = _context.Wishlists
                .FirstOrDefault(w => w.WishlistId == id);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
