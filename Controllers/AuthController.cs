using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Security_Bug_Reports.Models;
using Services.Hashing;
using Services.Token;
using Data;

namespace Security_Bug_Reports.Controllers
{
    [Route("")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthController(
            ApplicationDbContext db,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("register")]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LogAudit(Guid.Empty, "ValidationFailed_Register",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Email))
            {
                ModelState.AddModelError("", "This email is already registered.");
                await LogAudit(Guid.Empty, "RegisterFailed_Duplicate", $"Email taken: {model.Email}");
                return View(model);
            }

            _passwordHasher.CreateHash(model.Password, out var hash, out var salt);
            var user = new User
            {
                Username = model.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await LogAudit(user.Id, "RegisterSuccess", $"New user: {model.Email}");

            return RedirectToAction(nameof(Login));
        }

        [HttpGet("login")]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LogAudit(Guid.Empty, "ValidationFailed_Login",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == model.Email);
            if (user == null || !_passwordHasher.Verify(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                var action = user == null ? "LoginFailed_NoUser" : "LoginFailed_WrongPassword";
                await LogAudit(user?.Id ?? Guid.Empty, action, $"Attempt: {model.Email}");

                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            await LogAudit(user.Id, "LoginSuccess", $"User logged in: {model.Email}");
            var jwt = _tokenService.GenerateToken(user);
            Response.Cookies.Append(
                "AuthToken",
                jwt,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

            return RedirectToAction("Index", "Secure");
        }

        private async Task LogAudit(Guid userId, string action, string details)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
