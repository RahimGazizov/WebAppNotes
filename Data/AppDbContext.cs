using Microsoft.EntityFrameworkCore;
using NotesApp.Models;
namespace NotesApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Notes> Notes { get; set; }
    }
}
