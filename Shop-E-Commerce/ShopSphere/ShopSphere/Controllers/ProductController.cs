using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ShoppDbContext _context;

        public ProductsController(ShoppDbContext context)
        {
            _context = context;
        }

        // 1. PRODUCT LIST (SEARCH + SORT + FILTER)
      
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? categoryId, int? brandId)
        {
            var products = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    EF.Functions.Like(p.ProductName, $"%{searchString}%") ||
                    EF.Functions.Like(p.Description, $"%{searchString}%"));
            }

            // FILTER CATEGORY
            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // FILTER BRAND
            if (brandId != null)
            {
                products = products.Where(p => p.BrandId == brandId);
            }

            // SORTING
            switch (sortOrder)
            {
                case "price_low":
                    products = products.OrderBy(p => p.Price);
                    break;

                case "price_high":
                    products = products.OrderByDescending(p => p.Price);
                    break;

                case "name":
                    products = products.OrderBy(p => p.ProductName);
                    break;

                default:
                    products = products.OrderByDescending(p => p.ProductId);
                    break;
            }

            return View(await products.ToListAsync());
        }

       
        // 2. PRODUCT DETAILS
       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

      
        // 3. CREATE PRODUCT (RETAILER/ADMIN)
       
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.Status = "Pending";

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        
        // 4. EDIT PRODUCT
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

      
        // 5. DELETE PRODUCT
       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
