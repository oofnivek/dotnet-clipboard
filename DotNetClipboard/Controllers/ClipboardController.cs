using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetClipboard.Data;
using DotNetClipboard.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace DotNetClipboard.Controllers
{
    public class ClipboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _secretKey;
        private readonly IConfiguration _configuration;

        public ClipboardController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // Initialize
            _secretKey = _configuration.GetValue<string>("Recaptcha:SecretKey");
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
            string siteKey = _configuration.GetValue<string>("Recaptcha:SiteKey");
            if (string.IsNullOrEmpty(siteKey))
            {
                throw new InvalidOperationException("Recaptcha:SiteKey is missing in configuration.");
            }

            ViewData["RecaptchaSiteKey"] = siteKey;
            return View();
        }

        // POST: Clipboard/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("Id,Alias,Content")] Clipboard clipboard)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         _context.Add(clipboard);
        //         await _context.SaveChangesAsync();
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View(clipboard);
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Alias,Content")] Clipboard clipboard, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                string recaptchaResponse = form["g-recaptcha-response"]; // Get reCAPTCHA response

                if (string.IsNullOrEmpty(recaptchaResponse))
                {
                    ModelState.AddModelError("", "reCAPTCHA verification failed. Please complete the reCAPTCHA.");
                    return View(clipboard);
                }

                bool isValid = await VerifyRecaptcha(recaptchaResponse);

                if (isValid)
                {
                    _context.Add(clipboard);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
                    return View(clipboard);
                }
            }
            return View(clipboard);
        }

        private async Task<bool> VerifyRecaptcha(string recaptchaResponse)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("secret", _secretKey),
                new KeyValuePair<string, string>("response", recaptchaResponse)
            });

                try
                {
                    var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                    var responseJson = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"reCAPTCHA Response: {responseJson}"); // Log for debugging

                    var result = JObject.Parse(responseJson);

                    if (result["error-codes"] != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"reCAPTCHA Error: {result["error-codes"]}");
                        return false;
                    }

                    bool success = result["success"].Value<bool>();
                    return success; // For v2, just check success

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    return false;
                }
            }
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
