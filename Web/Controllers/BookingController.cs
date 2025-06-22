using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly WebdevP3Context _context;

        public BookingController(WebdevP3Context context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .ToListAsync();
            return View(bookings);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName");
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            Console.WriteLine("Create POST called 📝");
            Console.WriteLine($"BookingId: {booking?.BookingId}, EventId: {booking?.EventId}, VenueId: {booking?.VenueId}");

            if (booking == null)
            {
                Console.WriteLine("Booking object is null.");
                TempData["Error"] = "⚠️ Booking data is missing.";
                ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName");
                ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName");
                return View();
            }

            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (eventEntity == null)
            {
                Console.WriteLine("Event not found in DB.");
                TempData["Error"] = "⚠️ Selected event does not exist.";
                ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => $"❌ {e.ErrorMessage}")
                                              .ToList();
                Console.WriteLine(" ModelState Errors: " + string.Join(", ", errors));

                ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                TempData["Error"] = "⚠️ Validation failed. Please check the form.";
                return View(booking);
            }

            if (await IsVenueBookedOnDate(booking.VenueId, eventEntity.EventDate))
            {
                TempData["Error"] = "⚠️ This venue is already booked on the selected date.";
                ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            try
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Booking created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error saving booking: {ex.Message}");
                TempData["Error"] = "⚠️ Failed to save booking. Please try again.";
                return View(booking);
            }
        }


        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound("⚠️ Booking ID is required.");

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound("⚠️ Booking not found.");

            ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound("⚠️ Booking ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                // 🔥 Ensure dropdowns are repopulated when validation fails
                ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

                TempData["Error"] = "⚠️ Validation failed. Please check the form.";
                return View(booking);
            }

            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Booking updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bookings.Any(b => b.BookingId == booking.BookingId))
                {
                    return NotFound("⚠️ Booking not found.");
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Booking deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }

        private async Task<bool> IsVenueBookedOnDate(int venueId, DateTime eventDate, int? excludeBookingId = null)
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.VenueId == venueId && b.Event.EventDate == eventDate)
                .Where(b => excludeBookingId == null || b.BookingId != excludeBookingId)
                .AnyAsync();
        }

        //GET: Booking/SearchResult/5
        public async Task<IActionResult> SearchResults(string searchTerm, DateTime? bookingDate)
        {
            var query = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b =>
                    b.Event.EventName.Contains(searchTerm) ||
                    b.Venue.VenueName.Contains(searchTerm));
            }

            if (bookingDate.HasValue)
            {
                query = query.Where(b => b.BookingDate.Date == bookingDate.Value.Date);
            }

            var results = await query.ToListAsync();
            return View("Search", results);
        }
    }
}