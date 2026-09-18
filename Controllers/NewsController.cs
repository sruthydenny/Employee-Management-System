using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // GET: News
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var news = await _context.News
                .OrderByDescending(n => n.PublishedDate)
                .ToListAsync();

            return View(news);
        }


        // =====================================================
        // GET: News/Create
        // =====================================================

        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // POST: News/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news)
        {
            if (ModelState.IsValid)
            {
                news.PublishedDate = DateTime.Now;

                _context.News.Add(news);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "News item created successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(news);
        }


        // =====================================================
        // GET: News/Edit/5
        // =====================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var news = await _context.News
                .FindAsync(id);

            if (news == null)
                return NotFound();

            return View(news);
        }


        // =====================================================
        // POST: News/Edit/5
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            News news)
        {
            if (id != news.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingNews = await _context.News
                        .FindAsync(id);

                    if (existingNews == null)
                        return NotFound();

                    existingNews.Title = news.Title;
                    existingNews.Content = news.Content;
                    existingNews.IsImportant = news.IsImportant;

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "News item updated successfully.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.Id))
                        return NotFound();

                    throw;
                }
            }

            return View(news);
        }


        // =====================================================
        // GET: News/Delete/5
        // =====================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var news = await _context.News
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
                return NotFound();

            return View(news);
        }


        // =====================================================
        // POST: News/Delete/5
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News
                .FindAsync(id);

            if (news != null)
            {
                _context.News.Remove(news);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "News item deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // CHECK IF NEWS EXISTS
        // =====================================================

        private bool NewsExists(int id)
        {
            return _context.News
                .Any(n => n.Id == id);
        }
    }
}