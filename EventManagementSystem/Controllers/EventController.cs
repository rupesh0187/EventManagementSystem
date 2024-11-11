using EventManagementSystem.Data;
using EventManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagementSystem.Controllers
{
    [Authorize] // Ensure that only authenticated users can access this controller
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events - All users can see events
        public async Task<IActionResult> Index()
        {
            // Retrieve events where the Date string is not null or empty and is greater than or equal to the current date
            var events = await _context.Events.ToListAsync();

            events = events.Where(e => DateTime.Parse(e.Date) >= DateTime.Now).ToList();
            return View(events);
        }

        // GET: Create Event - Only Admins can create events
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Event
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Event eventModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventModel);
        }

        // GET: Edit Event - Only Admins can edit events
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }
            return View(eventModel);
        }

        // POST: Edit Event
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Event eventModel)
        {
            if (id != eventModel.Id)
            {
                return NotFound();
            }

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
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventModel);
        }

        // GET: Delete Event - Only Admins can delete events
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }
            return View(eventModel);
        }

        // POST: Delete Event
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await _context.Events.FindAsync(id);
            _context.Events.Remove(eventModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Registration for Users
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Register(int eventId)
        {
            var userId = User.Identity.Name;
            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.EventId == eventId);

            if (existingRegistration != null)
            {
                return RedirectToAction(nameof(Index));
            }

            var eventModel = await _context.Events.FindAsync(eventId);
            if (eventModel == null || eventModel.RegisteredParticipants >= eventModel.MaxParticipants)
            {
                return RedirectToAction(nameof(Index));
            }

            var registration = new Registration
            {
                EventId = eventId,
                UserId = userId
            };

            _context.Add(registration);
            eventModel.RegisteredParticipants++;
            _context.Update(eventModel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
