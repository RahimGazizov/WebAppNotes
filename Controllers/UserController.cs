using Microsoft.AspNetCore.Mvc;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.SendEmail;
using NotesApp.Service;
using System.Net.Mail;
using System.Security.Cryptography;
namespace NotesApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SettingEmail _settings;
        private readonly ResetPassword _reset;
        private readonly HashPassword.HashCode _hash;
        public UserController(AppDbContext context, SettingEmail setting, 
            ResetPassword reset,HashPassword.HashCode hash)
        {
            _context = context;
            _settings = setting;
            _reset = reset;
            _hash = hash;
        }
        public IActionResult Index(string? error)
        {
            //_context.Users.RemoveRange(_context.Users);
            //_context.SaveChanges();
            TempData["ErrorMessage"] = error;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            User hasEmail = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (hasEmail != null && !hasEmail.IsEmailConfirmed)
            {
                _context.Users.Remove(hasEmail);
                _context.SaveChanges();
                return View();
            }
            if (hasEmail != null && hasEmail.IsEmailConfirmed)
            {
                TempData["ErrorMessage"] = "Такая почта уже существует";
                return View();
            }
            var users = new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = _hash.HashPassword(user.Password),
                EmailConfirmationToken = Guid.NewGuid().ToString(),
                IsEmailConfirmed = false,
                EmailSent = DateTime.Now.AddMinutes(5),
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
            return RedirectToAction("CheckEmail", new { id = users.Id });
        }
        public IActionResult CheckEmail(int id)
        {
            User user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("Index", new { error = "Юзер был пустой" });
            }
            DateTime endTime = user.EmailSent;
            if (DateTime.Now > endTime)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                return RedirectToAction("Index", new { error = "Вы не подтвердили почту" });
            }
            return View(user);
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
            return RedirectToAction("CheckEmail", new { id = userId });
        }
        public IActionResult GetEmail() => View();
        [HttpPost]
        public IActionResult GetEmail(string email)
        {
            var result = _reset.StartReset(email);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View();
            }
            return RedirectToAction("ConfirmNum", new { id = result.Id });
        }
        public IActionResult ConfirmNum(int id)
        {
            User user = GetUser(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                return View();
            }
            return View(user);
        }
        [HttpPost]
        public IActionResult ConfirmNum(int id, int userNum)
        {
            var result = _reset.ConfNumber(id, userNum);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(GetUser(id));
            }
            return RedirectToAction("ResetPassword", new { id = result.Id });
        }
        public IActionResult ResetPassword(int id)
        {
            User user = GetUser(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                return View();
            }
            return View(user);
        }
        [HttpPost]
        public IActionResult ResetPassword(int id, string password, string confiredPassword)
        {
            if (password != confiredPassword)
            {
                TempData["ErrorMessage"] = "Пароли не совпадают";
                return View(GetUser(id));
            }
            var result = _reset.ResPassword(id, password);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return View(GetUser(id));
            }
            return RedirectToAction("Index", "Authorization");
        }
        private User GetUser(int id) => _context.Users.FirstOrDefault(u => u.Id == id);
        
    }
}
