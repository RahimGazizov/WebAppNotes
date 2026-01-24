using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Не корректный ввод почты")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string? EmailConfirmationToken {  get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime EmailSent { get; set; }
        public int? ResetPassword {  get; set; }
    }
}
