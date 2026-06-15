using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ShoppDbContext _context;

        public CategoryController(ShoppDbContext context)
        {
            _context = context;
        }

        // VIEW ALL CATEGORIES
        
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

       
        // CREATE CATEGORY (GET)
        
        public IActionResult Create()
        {
            return View();
        }

        // CREATE CATEGORY (POST)
     
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        
        // DELETE CATEGORY

        public IActionResult Delete(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
