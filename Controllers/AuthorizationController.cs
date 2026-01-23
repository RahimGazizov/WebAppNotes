using Microsoft.AspNetCore.Mvc;
using System.Net;
using NotesApp.Models;
using NotesApp.Data;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
namespace NotesApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly AppDbContext _context;
        public AuthorizationController(AppDbContext context)
        {
            _context = context;
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
            bool isVerification = VereficationPassword(authorization.Password, user.Password);
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
        private bool VereficationPassword(string password, string hashStand)
        {
            byte[] hashCode = Convert.FromBase64String(hashStand);
            byte[] salt = new byte[16];
            Array.Copy(hashCode, 0, salt, 0, 16);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashCode[i + 16] != hash[i])
                    return false;
            }
            return true;
        }
    }
}
