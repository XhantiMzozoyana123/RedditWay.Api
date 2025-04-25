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
    public class RedditAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedditAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RedditAccounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.RedditAccounts.ToListAsync());
        }

        // GET: RedditAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditAccounts = await _context.RedditAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditAccounts == null)
            {
                return NotFound();
            }

            return View(redditAccounts);
        }

        // GET: RedditAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RedditAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Password,ClientId,ClientSecret,Id,UserId,CreatedDate,UpdatedDate")] RedditAccounts redditAccounts)
        {
            if (ModelState.IsValid)
            {
                _context.Add(redditAccounts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redditAccounts);
        }

        // GET: RedditAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditAccounts = await _context.RedditAccounts.FindAsync(id);
            if (redditAccounts == null)
            {
                return NotFound();
            }
            return View(redditAccounts);
        }

        // POST: RedditAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserName,Password,ClientId,ClientSecret,Id,UserId,CreatedDate,UpdatedDate")] RedditAccounts redditAccounts)
        {
            if (id != redditAccounts.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redditAccounts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedditAccountsExists(redditAccounts.Id))
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
            return View(redditAccounts);
        }

        // GET: RedditAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditAccounts = await _context.RedditAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditAccounts == null)
            {
                return NotFound();
            }

            return View(redditAccounts);
        }

        // POST: RedditAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redditAccounts = await _context.RedditAccounts.FindAsync(id);
            if (redditAccounts != null)
            {
                _context.RedditAccounts.Remove(redditAccounts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedditAccountsExists(int id)
        {
            return _context.RedditAccounts.Any(e => e.Id == id);
        }
    }
}
