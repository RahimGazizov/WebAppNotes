using Microsoft.AspNetCore.Mvc;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.SendEmail;
using System.Net.Mail;
using System.Security.Cryptography;

namespace NotesApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SettingEmail _settings;
        public UserController(AppDbContext context, SettingEmail setting)
        {
            _context = context;
            _settings = setting;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            //_context.Users.RemoveRange(_context.Users);
            //_context.SaveChanges();
            User hasEmail = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (hasEmail != null)
            {
                TempData["ErrorMessage"] = "Такая почта уже существует";
                return View();
            }
            var users = new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = HashPassword(user.Password),
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                IsEmailConfirmed = false
            };
            _context.Users.Add(users);
            _context.SaveChanges();
            var confirmLink = Url.Action(
                "ConfirmEmail",
                "User",
                new { userId = users.Id, token = users.EmailConfirmationToken },
                Request.Scheme
                );
            try
            {
                await _settings.SendEmail(users.Email, confirmLink);
            }
            catch (SmtpFailedRecipientException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (SmtpException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (FormatException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction("CheckEmail");
        }
        public IActionResult CheckEmail()
        {
            return View();
        }
        public IActionResult ConfirmEmail(int userId, string token)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == userId && x.EmailConfirmationToken == token);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Ссылка не действительна";
                return View();
            }
            user.EmailConfirmationToken = null;
            user.IsEmailConfirmed = true;
            _context.SaveChanges();
            return RedirectToAction("Index","Authorization");
        }
        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rgb = RandomNumberGenerator.Create())
            {
                rgb.GetBytes(salt);
            }
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            byte[] hashCode = new byte[48];
            Array.Copy(salt, 0, hashCode, 0, 16);
            Array.Copy(hash, 0, hashCode, 16, hash.Length);
            return Convert.ToBase64String(hashCode);
        }
    }
}
