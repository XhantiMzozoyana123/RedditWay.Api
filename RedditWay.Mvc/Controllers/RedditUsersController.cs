using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RedditWay.Domain;
using RedditWay.Domain.Dtos;
using RedditWay.Domain.Entities;

namespace RedditWay.Mvc.Controllers
{
    public class RedditUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RedditUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RedditUsers
        public async Task<IActionResult> Index()
        {
            var list = await _context.RedditUsers.ToListAsync();
            ViewData["userCount"] = list.Count.ToString();
            return View(list);
        }

        // GET: RedditUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditUsers = await _context.RedditUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditUsers == null)
            {
                return NotFound();
            }

            return View(redditUsers);
        }

        // GET: RedditUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RedditUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,PostKarma,CommentKarma,Group,ExternalLink,Sent,CakeDate,Id,UserId,CreatedDate,UpdatedDate")] RedditUsers redditUsers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(redditUsers);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redditUsers);
        }

        // GET: RedditUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditUsers = await _context.RedditUsers.FindAsync(id);
            if (redditUsers == null)
            {
                return NotFound();
            }
            return View(redditUsers);
        }

        // POST: RedditUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserName,PostKarma,CommentKarma,Group,ExternalLink,Sent,CakeDate,Id,UserId,CreatedDate,UpdatedDate")] RedditUsers redditUsers)
        {
            if (id != redditUsers.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redditUsers);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedditUsersExists(redditUsers.Id))
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
            return View(redditUsers);
        }

        // GET: RedditUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditUsers = await _context.RedditUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (redditUsers == null)
            {
                return NotFound();
            }

            return View(redditUsers);
        }

        // POST: RedditUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redditUsers = await _context.RedditUsers.FindAsync(id);
            if (redditUsers != null)
            {
                _context.RedditUsers.Remove(redditUsers);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedditUsersExists(int id)
        {
            return _context.RedditUsers.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAll()
        {
            _context.RedditUsers.RemoveRange(_context.RedditUsers);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
