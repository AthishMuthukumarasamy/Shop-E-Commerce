using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ShoppDbContext _context;

        public AdminController(
    ShoppDbContext context,
    IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
                .Include(p => p.ProductImages)
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
        public IActionResult AddProduct(Product product, IFormFile productImage)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();

                return View(product);
            }

            if (productImage == null || productImage.Length == 0)
            {
                ModelState.AddModelError("productImage", "Product image is required");

                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();

                return View(product);
            }

            try
            {
                product.RetailerId = 1;
                product.Status = "Approved";
                product.CreatedDate = DateTime.Now;
                product.IsActive = true;

                _context.Products.Add(product);
                _context.SaveChanges();

                // Image Upload Code Here

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


                return RedirectToAction("Products");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error while saving product.");

                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Brands = _context.Brands.ToList();

                return View(product);
            }
        }

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

        // EDIT PRODUCT (POST) 
        [HttpPost]
        public IActionResult EditProduct(Product model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.Categories,
                    "CategoryId",
                    "CategoryName",
                    model.CategoryId);

                ViewBag.Brands = new SelectList(
                    _context.Brands,
                    "BrandId",
                    "BrandName",
                    model.BrandId);

                return View(model);
            }

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == model.ProductId);

            if (product == null)
                return NotFound();

            try
            {
                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.CategoryId = model.CategoryId;
                product.BrandId = model.BrandId;
                product.Status = model.Status;

                _context.SaveChanges();

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");

                ViewBag.Categories = new SelectList(
                    _context.Categories,
                    "CategoryId",
                    "CategoryName",
                    model.CategoryId);

                ViewBag.Brands = new SelectList(
                    _context.Brands,
                    "BrandId",
                    "BrandName",
                    model.BrandId);

                return View(model);
            }
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