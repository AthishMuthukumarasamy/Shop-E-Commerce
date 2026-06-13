using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class AdminController : Controller
    {
        private readonly ShoppDbContext _context;

        public AdminController(ShoppDbContext context)
        {
            _context = context;
        }

        
        // ADMIN DASHBOARD
       
        public IActionResult Index()
        {
            return View();
        }

        
        // PRODUCT LIST
        public IActionResult Products()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            return View(products);
        }

        
        // ADD PRODUCT (GET)
        
        public IActionResult AddProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View();
        }

        // ADD PRODUCT (POST)
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        
        // EDIT PRODUCT (GET)
        
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View(product);
        }

        // EDIT PRODUCT (POST)
        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        
        // DELETE PRODUCT
        
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("Products");
        }
    }
}