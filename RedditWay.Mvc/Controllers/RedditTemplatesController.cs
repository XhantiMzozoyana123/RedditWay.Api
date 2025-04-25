using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RedditWay.Domain;
using RedditWay.Domain.Entities;

namespace RedditWay.Mvc.Controllers
{
    public class RedditTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedditTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RedditTemplates
        public async Task<IActionResult> Index()
        {
            return View(await _context.RedditTemplates.ToListAsync());
        }

        // GET: RedditTemplates/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditTemplates = await _context.RedditTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditTemplates == null)
            {
                return NotFound();
            }

            return View(redditTemplates);
        }

        // GET: RedditTemplates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RedditTemplates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Subject,Body,Id,UserId,CreatedDate,UpdatedDate")] RedditTemplates redditTemplates)
        {
            if (ModelState.IsValid)
            {
                _context.Add(redditTemplates);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redditTemplates);
        }

        // GET: RedditTemplates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditTemplates = await _context.RedditTemplates.FindAsync(id);
            if (redditTemplates == null)
            {
                return NotFound();
            }
            return View(redditTemplates);
        }

        // POST: RedditTemplates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Subject,Body,Id,UserId,CreatedDate,UpdatedDate")] RedditTemplates redditTemplates)
        {
            if (id != redditTemplates.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redditTemplates);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedditTemplatesExists(redditTemplates.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(redditTemplates);
        }

        // GET: RedditTemplates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditTemplates = await _context.RedditTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditTemplates == null)
            {
                return NotFound();
            }

            return View(redditTemplates);
        }

        // POST: RedditTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redditTemplates = await _context.RedditTemplates.FindAsync(id);
            if (redditTemplates != null)
            {
                _context.RedditTemplates.Remove(redditTemplates);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedditTemplatesExists(int id)
        {
            return _context.RedditTemplates.Any(e => e.Id == id);
        }
    }
}
