using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class CartController : Controller
    {
        private readonly ShoppDbContext _context;

        public CartController(ShoppDbContext context)
        {
            _context = context;
        }

        
        // GET CART PAGE
        
        public IActionResult Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
            }

            return View(cart);
        }

        // ADD TO CART
       
        public IActionResult AddToCart(int productId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var product = _context.Products
                .FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
                return NotFound();

            // Get or create cart
            var cart = _context.Carts
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            // Check existing item
            var cartItem = _context.CartItems
                .FirstOrDefault(ci =>
                    ci.CartId == cart.CartId &&
                    ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = 1,
                    UnitPrice = product.Price
                };

                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        
        // REMOVE ITEM
       
        public IActionResult Remove(int id)
        {
            var item = _context.CartItems
                .FirstOrDefault(x => x.CartItemId == id);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

     
        // INCREASE QTY
       
        public IActionResult Increase(int id)
        {
            var item = _context.CartItems
                .FirstOrDefault(x => x.CartItemId == id);

            if (item != null)
            {
                item.Quantity++;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        
        // DECREASE QTY
        
        public IActionResult Decrease(int id)
        {
            var item = _context.CartItems
                .FirstOrDefault(x => x.CartItemId == id);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                    _context.CartItems.Remove(item);

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}