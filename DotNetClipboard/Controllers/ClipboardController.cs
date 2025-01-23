using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetClipboard.Data;
using DotNetClipboard.Models;

namespace DotNetClipboard.Controllers
{
    public class ClipboardController : Controller
    {
        private readonly AppDbContext _context;

        public ClipboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Clipboard
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clipboards.ToListAsync());
        }

        // GET: Clipboard/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clipboard = await _context.Clipboards
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clipboard == null)
            {
                return NotFound();
            }

            return View(clipboard);
        }

        // GET: Clipboard/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clipboard/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Alias,Content")] Clipboard clipboard)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clipboard);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(clipboard);
        }

        // GET: Clipboard/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clipboard = await _context.Clipboards.FindAsync(id);
            if (clipboard == null)
            {
                return NotFound();
            }
            return View(clipboard);
        }

        // POST: Clipboard/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Alias,Content")] Clipboard clipboard)
        {
            if (id != clipboard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clipboard);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClipboardExists(clipboard.Id))
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
            return View(clipboard);
        }

        // GET: Clipboard/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clipboard = await _context.Clipboards
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clipboard == null)
            {
                return NotFound();
            }

            return View(clipboard);
        }

        // POST: Clipboard/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clipboard = await _context.Clipboards.FindAsync(id);
            if (clipboard != null)
            {
                _context.Clipboards.Remove(clipboard);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClipboardExists(int id)
        {
            return _context.Clipboards.Any(e => e.Id == id);
        }
    }
}
