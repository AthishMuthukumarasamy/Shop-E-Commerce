using Microsoft.AspNetCore.Mvc;
using ShopSphere.DatabaseModels;

namespace ShopSphere.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly ShoppDbContext _context;

        public AccountApiController(ShoppDbContext context)
        {
            _context = context;
        }

        // REGISTER API
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = _context.Users.Any(x => x.Email == user.Email);

            if (exists)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email already exists"
                });
            }

            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Registration successful"
            });
        }

        // LOGIN API
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(x =>
                x.Email == request.Email &&
                x.PasswordHash == request.Password);

            if (user == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Email or Password"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Login successful",
                data = new
                {
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }

        // GET PROFILE
        [HttpGet("profile/{id}")]
        public IActionResult Profile(int id)
        {
            var user = _context.Users
                .Where(x => x.UserId == id)
                .Select(x => new
                {
                    x.UserId,
                    x.Name,
                    x.Email,
                    x.Role,
                    x.CreatedDate
                })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
