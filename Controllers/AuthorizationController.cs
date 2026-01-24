using Microsoft.AspNetCore.Mvc;
using System.Net;
using NotesApp.Models;
using NotesApp.Data;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using NotesApp.SendEmail;
namespace NotesApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly HashPassword.HashCode _hash;
        public AuthorizationController(AppDbContext context, HashPassword.HashCode hash)
        {
            _context = context;
            _hash = hash;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(AuthorizationModel authorization)
        {
            var user = _context.Users.FirstOrDefault(e => e.Email.ToLower().Trim()
            == authorization.Login.ToLower().Trim());
            if (user == null)
            {
                TempData["ErrorMessage"] = "Такого пользователя не существует";
                return View();
            }
            bool isVerification = _hash.VereficationPassword(authorization.Password, user.Password);
            if (!isVerification)
            {
                TempData["ErrorMessage"] = "Неверный логин или пароль";
                return View();
            }

            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user?.Id.ToString() ?? "0")
            };
            var identity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Notes");
        }
        
      
    }
}
