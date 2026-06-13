using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class RetailerController : Controller
    {
        private readonly ShoppDbContext _context;

        public RetailerController(ShoppDbContext context)
        {
            _context = context;
        }

        // SESSION HELPER
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // DASHBOARD
        public IActionResult Index()
        {
            int userId = GetUserId();

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            ViewBag.TotalProducts = _context.Products.Count(p => p.RetailerId == userId);
            ViewBag.PendingProducts = _context.Products.Count(p => p.RetailerId == userId && p.Status == "Pending");
            ViewBag.ApprovedProducts = _context.Products.Count(p => p.RetailerId == userId && p.Status == "Approved");

            return View();
        }

        // MY PRODUCTS
        public IActionResult MyProducts()
        {
            int userId = GetUserId();

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.RetailerId == userId)
                .ToList();

            return View(products);
        }

        // CREATE
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            int userId = GetUserId();

            product.RetailerId = userId;
            product.CreatedDate = DateTime.Now;
            product.Status = "Pending";

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("MyProducts");
        }

        // EDIT
        public IActionResult Edit(int id)
        {
            int userId = GetUserId();

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && p.RetailerId == userId);

            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("MyProducts");
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            int userId = GetUserId();

            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefault(p => p.ProductId == id && p.RetailerId == userId);

            if (product == null) return NotFound();

            return View(product);
        }

        // DELETE (CONFIRM PAGE)
        public IActionResult Delete(int id)
        {
            int userId = GetUserId();

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && p.RetailerId == userId);

            if (product == null) return NotFound();

            return View(product);
        }

        // DELETE CONFIRMED
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            int userId = GetUserId();

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && p.RetailerId == userId);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("MyProducts");
        }
    }
}