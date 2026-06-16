using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // SECURITY CHECK METHOD
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

            product.RetailerId = 1; // FIX HERE
            product.Status = "Approved";
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Products");
        }

        // EDIT PRODUCT
        //public IActionResult EditProduct(int id)
        //{
        //    if (!IsAdmin())
        //        return RedirectToAction("Login", "Account");

        //    var product = _context.Products.Find(id);
        //    if (product == null)
        //        return NotFound();

        //    ViewBag.Categories = _context.Categories.ToList();
        //    ViewBag.Brands = _context.Brands.ToList();

        //    return View(product);
        //}

        public IActionResult EditProduct(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName",
                product.CategoryId);

            ViewBag.Brands = new SelectList(
                _context.Brands,
                "BrandId",
                "BrandName",
                product.BrandId);

            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product model)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProductId == model.ProductId);

            if (product == null)
                return NotFound();

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            //product.CategoryId = model.CategoryId;
            //product.BrandId = model.BrandId;
            //product.RetailerId = model.RetailerId;
            product.Status = model.Status;
            //product.IsActive = model.IsActive;
            //product.IsActive = model.IsActive;

            if (string.IsNullOrEmpty(product.Status))
            {
                product.Status = "Pending";
            }
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
            var product = _context.Products.Find(id);

            if (product != null)
            {
                // STEP 1: delete child records first
                var images = _context.ProductImages
                    .Where(x => x.ProductId == id)
                    .ToList();

                _context.ProductImages.RemoveRange(images);

                // STEP 2: delete product
                _context.Products.Remove(product);

                _context.SaveChanges();
            }

            return RedirectToAction("Products");
        }
    }
}