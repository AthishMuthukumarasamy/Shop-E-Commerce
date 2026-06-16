using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShoppDbContext _context;

        public AccountController(ShoppDbContext context)
        {
            _context = context;
        }

        
        // REGISTER (GET)
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            var exists = _context.Users.Any(x => x.Email == user.Email);

            if (exists)
            {
                ViewBag.Error = "Email already exists";
                return View(user);
            }

            //  Role comes from dropdown now
            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "User";
            }

            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
        // LOGIN (GET)

        public IActionResult Login()
        {
            return View();
        }

     
        // LOGIN (POST)
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email && x.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View();
            }

          
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role ?? "");
            HttpContext.Session.SetString("UserName", user.Name ?? "");

            // ROLE REDIRECT
            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            if (user.Role == "Retailer")
                return RedirectToAction("Index", "Retailer");

            return RedirectToAction("Index", "Home");
        }

        
        // LOGOUT
       
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // PROFILE
      
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }
    }
}