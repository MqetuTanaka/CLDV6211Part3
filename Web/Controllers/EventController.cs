using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Controllers
{
    public class EventController : Controller
    {
        private readonly WebdevP3Context _context;

        public EventController(WebdevP3Context context)
        {
            _context = context;
        }

        // GET: Event/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null) return NotFound();

            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Event updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            TempData["Error"] = "⚠️ Validation failed. Please check the form.";
            return View(@event);
        }

        private bool EventExists(int eventId)
        {
            return _context.Events.Any(e => e.EventId == eventId);
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            var eventsGroupedByVenue = await _context.Events
                .Include(e => e.Venue)
                .OrderBy(e => e.Venue.VenueName)
                .ThenBy(e => e.EventDate)
                .ToListAsync();

            return View(eventsGroupedByVenue);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            Console.WriteLine("Create POST action hit.");
            Console.WriteLine($"EventName: {@event.EventName}");
            Console.WriteLine($"EventDate: {@event.EventDate}");
            Console.WriteLine($"Description: {@event.Description}");
            Console.WriteLine($"VenueId: {@event.VenueId}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid. Attempting to save...");

                _context.Add(@event);
                await _context.SaveChangesAsync();

                Console.WriteLine("Event saved successfully.");
                TempData["Success"] = "✅ Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("ModelState is INVALID. Errors:");
            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"- Field '{kvp.Key}': {error.ErrorMessage}");
                }
            }

            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            TempData["Error"] = "⚠️ Validation failed. Please check the form.";
            return View(@event);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
                return NotFound();

            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            try
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Event deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("FK_Booking_Event"))
                {
                    TempData["DeleteError"] = "⚠️ This event cannot be deleted because it has existing bookings.";
                }
                else
                {
                    TempData["DeleteError"] = "❌ An error occurred while deleting the event.";
                }
                return View("Delete", eventItem); // 🔥 Stay on the Delete page and show the error
            }
        }
    }
}