using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    public class AuthorizationModel
    {
        [EmailAddress(ErrorMessage = "Не корректный ввод почты")]
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
