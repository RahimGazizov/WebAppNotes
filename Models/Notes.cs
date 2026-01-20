namespace NotesApp.Models
{
    public class Notes
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User UserName { get; set; }
    }
}
