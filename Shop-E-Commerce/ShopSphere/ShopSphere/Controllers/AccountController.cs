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
                ViewBag.Error = "Invalid Email or Password";
                return View();
            }

            // SESSION
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role ?? "");
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

            return View(user);
        }
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

            // generate OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new Otp
            {
                UserId = user.UserId,
                OtpCode = otpCode,
                ExpiryTime = DateTime.Now.AddMinutes(5),
                IsUsed = false
            };

            _context.Otps.Add(otp);
            _context.SaveChanges();

            // TEMP (replace with email sending later)
            TempData["UserId"] = user.UserId;
            TempData["Otp"] = otpCode;

            return RedirectToAction("VerifyOtp");
        }
        public IActionResult VerifyOtp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult VerifyOtp(int userId, string otpCode)
        {
            var otp = _context.Otps
                .Where(x => x.UserId == userId
                         && x.OtpCode == otpCode
                         && x.IsUsed == false
                         && x.ExpiryTime > DateTime.Now)
                .OrderByDescending(x => x.OtpId)
                .FirstOrDefault();

            if (otp == null)
            {
                ViewBag.Error = "Invalid or expired OTP";
                return View();
            }

            otp.IsUsed = true;
            _context.SaveChanges();

            // store user for reset
            TempData["UserId"] = userId;

            return RedirectToAction("ResetPassword");
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ResetPassword(string newPassword)
        {
            if (TempData["UserId"] == null)
                return RedirectToAction("Login");

            int userId = Convert.ToInt32(TempData["UserId"]);

            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
                return NotFound();

            //  later replace with hashing
            user.PasswordHash = newPassword;

            _context.SaveChanges();

            return RedirectToAction("Login");
        }


    }
}