using EventManagementSystem.Data;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            // Retrieve events where the Date string is not null or empty and is greater than or equal to the current date
            var events = await _context.Events
                .Where(e => !string.IsNullOrEmpty(e.Date) &&
                            string.Compare(e.Date, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"), StringComparison.Ordinal) >= 0)
                .ToListAsync();

            return View(events);
        }

        // GET: Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Event eventModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(eventModel.Description))
                {
                    eventModel.Description = "No description provided";  // Optional: Set a default value if Description is null
                }

                _context.Add(eventModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventModel);
        }

        // GET: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null)
                return NotFound();

            return View(eventModel);
        }

        // POST: Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Event eventModel)
        {
            if (id != eventModel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Events.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventModel);
        }

        // GET: Delete
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null)
                return NotFound();

            return View(eventModel);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            _context.Events.Remove(eventModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // User Registration for Events
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Register(int eventId)
        {
            var userId = User.Identity.Name;

            // Check if the user is already registered for the event
            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);

            if (existingRegistration != null)
                return RedirectToAction(nameof(Index));

            var eventModel = await _context.Events.FindAsync(eventId);
            if (eventModel == null || eventModel.RegisteredParticipants >= eventModel.MaxParticipants)
                return RedirectToAction(nameof(Index));

            // Register the user for the event
            var registration = new Registration
            {
                EventId = eventId,
                UserId = userId
            };
            _context.Add(registration);

            // Update the registered participants count
            eventModel.RegisteredParticipants++;
            _context.Update(eventModel);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
