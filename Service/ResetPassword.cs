using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.SendEmail;
using System.Runtime;
namespace NotesApp.Service
{
    public class ResetPassword
    {
        private readonly AppDbContext _context;
        private readonly SettingEmail _setting;
        private readonly HashPassword.HashCode _hash;
        public ResetPassword(AppDbContext context, SettingEmail setting, HashPassword.HashCode hash)
        {
            _context = context;
            _setting = setting;
            _hash = hash;
        }
        public class OperationResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public int? Id { get; set; }
            public static OperationResult Ok(int? id = null) => new OperationResult { Success = true, Id = id };
            public static OperationResult Fail(string message) => new OperationResult { Success = false, Message = message };
        }

        public OperationResult StartReset(string email)
        {
            User user = _context.Users.FirstOrDefault(u => u.Email.ToLower().Trim() == email.ToLower().Trim());
            if (user == null)
                return OperationResult.Fail("Пользователь не найден");

            Random random = new Random();
            int randNum = random.Next(1000, 9999);
            user.ResetPassword = randNum;
            _context.Users.Update(user);
            _context.SaveChanges();
            _setting.SendEmail(email, Convert.ToString(randNum));
            return OperationResult.Ok(user.Id);
        }
        public OperationResult ConfNumber(int id, int userNum)
        {
            User user = GetUser(id);
            if (userNum != user.ResetPassword)
                return OperationResult.Fail("Вы вели не верное число");

            if (user == null)
                return OperationResult.Fail("Пользователь не найден");

            return OperationResult.Ok(user.Id);
        }
        public OperationResult ResPassword(int id, string password)
        {
            User user = GetUser(id);
            if (password == user.Password)
                return OperationResult.Fail("Новый пароль не должен быть похож на старый пароль");
            if (_hash.VereficationPassword(password, user.Password))
                return OperationResult.Fail("Пароль не должен быть похож на старый");
            user.Password = _hash.HashPassword(password);
            user.ResetPassword = null;
            _context.Users.Update(user);
            _context.SaveChanges();
            _setting.SendEmail(user.Email, "Пароль успешно изменен");
            return OperationResult.Ok();
        }
        private User GetUser(int id) => _context.Users.FirstOrDefault(u => u.Id == id);
    }
}
