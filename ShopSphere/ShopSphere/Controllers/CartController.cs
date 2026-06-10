using Microsoft.AspNetCore.Mvc;
using ShopSphere.Data;
using ShopSphere.Extensions;
using ShopSphere.Models;

namespace ShopSphere.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            return View(cart);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            var cart = HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }
    }
}