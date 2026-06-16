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

        // 🔐 SECURITY CHECK METHOD
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // ADMIN DASHBOARD
        public IActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // PRODUCT LIST
        public IActionResult Products()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Retailer)
                .ToList();

            return View(products);
        }

        // VIEW PRODUCT DETAILS
        public IActionResult ViewProduct(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Retailer)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ADD PRODUCT (GET)
        public IActionResult AddProduct()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();
                return View(product);
            }

            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        // EDIT PRODUCT
        public IActionResult EditProduct(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();
                return View(product);
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        // DELETE PRODUCT
        public IActionResult DeleteProduct(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

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