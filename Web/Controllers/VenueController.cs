using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Controllers
{
    public class VenueController : Controller
    {
        private readonly WebdevP3Context _context;
        private readonly string _connectionString;
        private readonly string _containerName = "wbdevp3";

        public VenueController(WebdevP3Context context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("AzureStorageConnectionString");
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync();
            return View(venues);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? eventType, string location, bool? availability)
        {
            ViewBag.EventTypes = await _context.EventTypes.ToListAsync();

            ViewBag.SelectedEventType = eventType;
            ViewBag.Location = location;  // new line for location filter
            ViewBag.Availability = availability;


            var query = _context.Venues.Include(v => v.EventType).AsQueryable();

            if (eventType.HasValue)
                query = query.Where(v => v.EventTypeId == eventType.Value);

            if (availability.HasValue)
                query = query.Where(v => v.IsAvailable == availability.Value);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(v => v.Location.Contains(location));

            var venues = await query.ToListAsync();
            return View(venues);

        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound("⚠️ ID is required.");

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null) return NotFound("⚠️ Venue not found.");

            return View(venue);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        var blobServiceClient = new BlobServiceClient(_connectionString);
                        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                        var blobClient = containerClient.GetBlobClient(venue.ImageFile.FileName);

                        using (var stream = venue.ImageFile.OpenReadStream())
                        {
                            await blobClient.UploadAsync(stream, true);
                        }

                        venue.ImageUrl = blobClient.Uri.ToString();
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Venue created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"⚠️ Failed to create venue. Error: {ex.Message}";
                    return View(venue);
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => $"❌ {e.ErrorMessage}")
                                          .ToList();
            Console.WriteLine(" ModelState Errors: " + string.Join(", ", errors));
            TempData["Error"] = "⚠️ Validation failed. Please check the form.";
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound("⚠️ ID is required.");

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound("⚠️ Venue not found.");

            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueId)
            {
                return NotFound("⚠️ ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => $"❌ {e.ErrorMessage}")
                                              .ToList();
                Console.WriteLine(" ModelState Errors: " + string.Join(", ", errors));
                TempData["Error"] = "⚠️ Validation failed. Please check the form.";
                return View(venue);
            }

            try
            {
                if (venue.ImageFile != null)
                {
                    var blobServiceClient = new BlobServiceClient(_connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                    var blobClient = containerClient.GetBlobClient(venue.ImageFile.FileName);

                    using (var stream = venue.ImageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    venue.ImageUrl = blobClient.Uri.ToString();
                }

                _context.Update(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Venue updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId))
                {
                    return NotFound("⚠️ Venue not found.");
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound("⚠️ ID is required.");

            var venue = await _context.Venues
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null) return NotFound("⚠️ Venue not found.");

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.Include(v => v.Events).FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            try
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Venue deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("FK_Event_Venue"))
                {
                    ViewBag.DeleteError = "⚠️ This venue cannot be deleted because it is being used in events.";
                }
                else
                {
                    ViewBag.DeleteError = "❌ An error occurred while deleting the venue. Check if it is being used";
                }

                return View("Delete", venue); // 🔥 Instead of redirecting, return the same Delete view
            }
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}