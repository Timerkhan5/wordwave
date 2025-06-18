using Microsoft.AspNetCore.Mvc;
using wordwave.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace wordwave.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest(new { error = "Логин и пароль обязательны" });

                if (_db.Users.Any(u => u.UserName == req.UserName))
                    return BadRequest(new { error = "Пользователь уже существует" });

                var user = new User
                {
                    UserName = req.UserName,
                    PasswordHash = HashPassword(req.Password),
                    RegisteredAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка регистрации: " + ex.ToString());
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest(new { error = "Логин и пароль обязательны" });

                var passwordHash = HashPassword(req.Password);
                var user = _db.Users.FirstOrDefault(u => u.UserName == req.UserName && u.PasswordHash == passwordHash);
                if (user == null)
                    return BadRequest(new { error = "Неверный логин или пароль" });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role ?? "user")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return Ok(new { success = true, userName = user.UserName });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка входа: " + ex.ToString());
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true });
        }

        public class RegisterRequest
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
