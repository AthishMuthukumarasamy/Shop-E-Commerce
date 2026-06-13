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

     
        // REGISTER
      
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
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

            user.Role = "User";
            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

       
        // LOGIN
       
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.Email == email && x.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email and Password";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role ?? "");
            HttpContext.Session.SetString("UserName", user.Name ?? "");

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

            return View(user);
        }

       
        // FORGOT PASSWORD (OTP)
       
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Email not found";
                return View();
            }

            var otp = new Otp
            {
                UserId = user.UserId,
                OtpCode = new Random().Next(100000, 999999).ToString(),
                ExpiryTime = DateTime.Now.AddMinutes(5),
                IsUsed = false
            };

            _context.Otps.Add(otp);
            _context.SaveChanges();

            ViewBag.Message = "OTP generated (demo): " + otp.OtpCode;

            return RedirectToAction("ResetPassword", new { userId = user.UserId });
        }

      
        // RESET PASSWORD
      
        public IActionResult ResetPassword(int userId)
        {
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(int userId, string otpCode, string newPassword)
        {
            var otp = _context.Otps.FirstOrDefault(x =>
                x.UserId == userId &&
                x.OtpCode == otpCode &&
                x.IsUsed == false);

            if (otp == null || otp.ExpiryTime < DateTime.Now)
            {
                ViewBag.Error = "Invalid or expired OTP";
                ViewBag.UserId = userId;
                return View();
            }

            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

            if (user != null)
            {
                user.PasswordHash = newPassword;
                otp.IsUsed = true;

                _context.SaveChanges();
            }

            return RedirectToAction("Login");
        }
    }
}