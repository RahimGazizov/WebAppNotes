using Microsoft.AspNetCore.Mvc;
using NotesApp.Data;
using NotesApp.Models;
using System.Security.Claims;

namespace NotesApp.Controllers
{
    public class NotesController : Controller
    {
        private readonly AppDbContext _context;
        public NotesController(AppDbContext context)
        {
            _context = context;
        }
        private int CurrentId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public IActionResult Index()
        {
            //_context.Notes.RemoveRange(_context.Notes);
            //_context.SaveChanges();
            int userId = CurrentId();
            var notes = _context.Notes.Where(n => n.UserId == userId).ToList();
            return View(notes);
        }
        public IActionResult AddNewNote()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddNewNote(Notes notes)
        {
            int userId = CurrentId();
            var note = new Notes
            {
                Title = notes.Title,
                Content = notes.Content,
                UserId = userId
            };
            Console.WriteLine(userId);
            _context.Notes.Add(note);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var note = _context.Notes.FirstOrDefault(n => n.Id == id);
            _context.Notes.Remove(note);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var note = _context.Notes.FirstOrDefault(o => o.Id == id);
            return View(note);
        }
        [HttpPost]
        public IActionResult Edit(Notes notes)
        {
            int userId = CurrentId();
            var existingNote = _context.Notes.FirstOrDefault(n => n.Id == notes.Id && n.UserId == userId);

            if (existingNote != null)
            {
                existingNote.Title = notes.Title;
                existingNote.Content = notes.Content;
                existingNote.Date = DateTime.Now;

                _context.Notes.Update(existingNote);
                _context.SaveChanges();
            }
            return View();
        }
    }
}
