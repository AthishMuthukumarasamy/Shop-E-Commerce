using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class CompareController : Controller
    {
        private readonly ShoppDbContext _context;

        public CompareController(ShoppDbContext context)
        {
            _context = context;
        }

        
        // VIEW COMPARE LIST
      
        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var compareList = _context.CompareProducts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            return View(compareList);
        }

       
        // ADD TO COMPARE
        
        public IActionResult Add(int productId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var exists = _context.CompareProducts
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (exists == null)
            {
                _context.CompareProducts.Add(new CompareProduct
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedDate = DateTime.Now
                });

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // REMOVE FROM COMPARE
       
        public IActionResult Remove(int id)
        {
            var item = _context.CompareProducts.FirstOrDefault(c => c.CompareId == id);

            if (item != null)
            {
                _context.CompareProducts.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
