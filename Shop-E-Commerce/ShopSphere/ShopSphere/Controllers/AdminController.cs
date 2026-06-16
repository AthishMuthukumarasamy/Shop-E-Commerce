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

        // SECURITY CHECK
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // DASHBOARD
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

        // VIEW PRODUCT
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

        // ADD PRODUCT (POST)
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            // FIX: use real admin session or remove retailer dependency
            product.RetailerId = 0; // or set null if DB allows

            product.Status = "Approved";
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            // VALIDATE FK BEFORE INSERT
            if (!_context.Categories.Any(c => c.CategoryId == product.CategoryId))
                return BadRequest("Invalid Category");

            if (!_context.Brands.Any(b => b.BrandId == product.BrandId))
                return BadRequest("Invalid Brand");

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        // EDIT PRODUCT (GET)
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

        // EDIT PRODUCT (POST) - FIXED
        [HttpPost]
        public IActionResult EditProduct(Product model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            int id = model.ProductId;
            int categoryId = model.CategoryId;
            int brandId = model.BrandId;

            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.CategoryId = categoryId;
            product.BrandId = brandId;

            _context.SaveChanges();

            return RedirectToAction("Products");
        }
        // DELETE PRODUCT (CONFIRM PAGE)
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

        // DELETE CONFIRMED (SAFE DELETE)
        [HttpPost, ActionName("DeleteProduct")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var product = _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                // delete child tables first (prevents FK errors)
                _context.ProductImages.RemoveRange(product.ProductImages);

                _context.CartItems.RemoveRange(
                    _context.CartItems.Where(x => x.ProductId == id));

                _context.Wishlists.RemoveRange(
                    _context.Wishlists.Where(x => x.ProductId == id));

                _context.OrderDetails.RemoveRange(
                    _context.OrderDetails.Where(x => x.ProductId == id));

                _context.Products.Remove(product);

                _context.SaveChanges();
            }

            return RedirectToAction("Products");
        }
    }
}