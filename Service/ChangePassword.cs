using NotesApp.Data;
using NotesApp.Models;
using NotesApp.SendEmail;
using NotesApp.ViewModels;
namespace NotesApp.Service
{
    public class ChangePassword
    {
        private readonly AppDbContext _context;
        private readonly HashPassword.HashCode _hashCode;
        private readonly SettingEmail _setting;
        public ChangePassword(AppDbContext context, HashPassword.HashCode hashCode,SettingEmail setting)
        {
            _context = context;
            _hashCode = hashCode;
            _setting = setting;
        }
        public class ResultOper
        {
            public bool Success { get; set; }
            public string Message { get; set; }

            public static ResultOper Ok() => new ResultOper { Success = true };
            public static ResultOper Fail(string message) => new ResultOper { Success = false, Message = message };
        }
        public ResultOper Change(ProfileViewModel profile)
        {
            User user = _context.Users.Find(profile.User.Id);
            if (user == null)
                return ResultOper.Fail("Пользователь не найден");
            if (!_hashCode.VereficationPassword(profile.Password.OldPassword, user.Password))
                return ResultOper.Fail("Не верный пароль");
            if (profile.Password.NewPassword != profile.Password.ConfirmPassword)
                return ResultOper.Fail("Пароли не совпадают");
            user.Password = _hashCode.HashPassword(profile.Password.NewPassword);
            _context.Users.Update(user);
            _context.SaveChanges();
            _setting.SendEmail(user.Email, "Пароль изменен");
            return ResultOper.Ok();
        }
    }

}
