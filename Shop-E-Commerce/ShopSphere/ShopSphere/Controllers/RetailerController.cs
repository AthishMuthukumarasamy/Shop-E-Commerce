using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class RetailerController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ShoppDbContext _context;

        public RetailerController(
            ShoppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
                .Include(p => p.ProductImages)
                .Where(p => p.RetailerId == userId)
                .ToList();

            return View(products);
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        public IActionResult Create(Product product, IFormFile productImage)
        {
            //int userId = GetUserId();

            //if (userId == 0)
            //    return RedirectToAction("Login", "Account");

            //product.RetailerId = userId;
            //product.CreatedDate = DateTime.Now;
            //product.Status = "Pending";
            //product.IsActive = true;

            //_context.Products.Add(product);
            //_context.SaveChanges();

            int userId = GetUserId();

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();

                return View(product);
            }

            product.RetailerId = userId;
            product.CreatedDate = DateTime.Now;
            product.Status = "Pending";
            product.IsActive = true;

            _context.Products.Add(product);
            _context.SaveChanges();



            if (productImage != null && productImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "Images",
                    "ProductImage");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName =
                    Guid.NewGuid().ToString() +
                    Path.GetExtension(productImage.FileName);

                string filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    productImage.CopyTo(stream);
                }

                ProductImage image = new ProductImage
                {
                    ProductId = product.ProductId,
                    ImageUrl = fileName,
                    IsPrimary = true
                };

                _context.ProductImages.Add(image);
                _context.SaveChanges();
            }

            return RedirectToAction("MyProducts");
        }

        // EDIT (GET)
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

        // EDIT (POST)
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            int userId = GetUserId();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();

                return View(product);
            }

            var existingProduct = _context.Products
                .FirstOrDefault(p => p.ProductId == product.ProductId
                                  && p.RetailerId == userId);

            if (existingProduct == null)
                return NotFound();

            try
            {
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;

                _context.SaveChanges();

                return RedirectToAction("MyProducts");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the product.");

                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();

                return View(product);
            }
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

        // DELETE CONFIRMED (FIXED FK ISSUE)
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            int userId = GetUserId();

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == id && p.RetailerId == userId);

            if (product != null)
            {
                // ELETE CHILD TABLES FIRST
                var images = _context.ProductImages.Where(x => x.ProductId == id).ToList();
                _context.ProductImages.RemoveRange(images);

                var wishlist = _context.Wishlists.Where(x => x.ProductId == id).ToList();
                _context.Wishlists.RemoveRange(wishlist);

                var cartItems = _context.CartItems.Where(x => x.ProductId == id).ToList();
                _context.CartItems.RemoveRange(cartItems);

                var orderDetails = _context.OrderDetails.Where(x => x.ProductId == id).ToList();
                _context.OrderDetails.RemoveRange(orderDetails);

                // DELETE PRODUCT
                _context.Products.Remove(product);

                _context.SaveChanges();
            }

            return RedirectToAction("MyProducts");
        }
    }
}