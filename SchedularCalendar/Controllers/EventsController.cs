using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedularCalendar.Authorisation;
using SchedularCalendar.Data;
using SchedularCalendar.Helpers;
using SchedularCalendar.Models;

namespace SchedularCalendar.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Events
        public async Task<IActionResult> Index(int pg=1)
        {
            List<Event> eventsList = _context.events.ToList();

            var isManager = User.IsInRole(Constants.ManagerRole);
            if (isManager == false)
            {
                var creatorEvents = eventsList.Where(e => e.CreatorId == _userManager.GetUserId(User));
                
                var april = new DateTime(2000, 04, 01);
                int totalDaysOffThisYear = 0;
                foreach (var item in creatorEvents)
                {
                    if (item.StartDate.Month > april.Month && item.StartDate.Year > DateTime.Now.Year - 1 && item.Status == EventStatus.Approved)
                    {
                        totalDaysOffThisYear += item.Days;
                    }

                    if (item.StartDate.Month < april.Month && item.StartDate.Year == DateTime.Now.Year + 1 && item.Status == EventStatus.Approved)
                    {
                        totalDaysOffThisYear += item.Days;
                    }

                    if (item.StartDate.Month < april.Month && item.EndDate.Month == april.Month &&item.StartDate.Year == DateTime.Now.Year - 1 && item.Status == EventStatus.Approved)
                    {
                        var leaveEndDay = item.EndDate.Day;

                        totalDaysOffThisYear += leaveEndDay;
                    }
                }

                const int pageSize = 20;
                if (pg < 1)
                    pg = 1;

                int recCount = creatorEvents.Count();
                var pager = new PagerHelper(recCount, pg, pageSize);

                int recSkip = (pg - 1) * pageSize;

                var data = creatorEvents.Skip(recSkip).Take(pager.PageSize).ToList();

                this.ViewBag.PagerHelper = pager;
                if (totalDaysOffThisYear > 0)
                    this.ViewBag.totalDaysOffThisYear = totalDaysOffThisYear;

                return View(data);
            }
            else
            {
                //Paging
                const int pageSize = 10;
                if (pg < 1)
                    pg = 1;

                int recCount = eventsList.Count;
                var pager = new PagerHelper(recCount, pg, pageSize);

                int recSkip = (pg - 1) * pageSize;

                var data = eventsList.Skip(recSkip).Take(pager.PageSize).ToList();

                this.ViewBag.PagerHelper = pager;

                return View(data);
            }
        }


        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.events == null)
            {
                return NotFound();
            }

            var @event = await _context.events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            var isCreator = await _authorizationService.AuthorizeAsync(
                User, @event, EventsOperations.Read);

            var isManager = User.IsInRole(Constants.ManagerRole);
            var isAdmin = User.IsInRole(Constants.AdminRole);
            if (isCreator.Succeeded == false && isManager == false && isAdmin == false)
                return Forbid();

            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, EventStatus status)
        {
            var @event = await _context.events.FindAsync(id);
            if (@event == null)
                return NotFound();

            var eventOperation = status == EventStatus.Approved
               ? EventsOperations.Approve
               : EventsOperations.Reject;
            var isAuthorised = await _authorizationService.AuthorizeAsync(
                User, @event, eventOperation);

            if (isAuthorised.Succeeded == false)
                return Forbid();

            @event.Status = status;
            _context.events.Update(@event);

            await _context.SaveChangesAsync();

            return View(@event);

        }
        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,StartDate,EndDate,CreatorId")] Event @event)
        {
            @event.CreatorId = _userManager.GetUserId(User);
            @event.Status = EventStatus.Pending;

            //calculate working days except bank holidays
            double calcBusinessDays =
                1 + ((@event.EndDate - @event.StartDate).TotalDays * 5 -
                (@event.StartDate.DayOfWeek - @event.EndDate.DayOfWeek) * 2) / 7;

            if (@event.EndDate.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (@event.StartDate.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            @event.Days = Convert.ToInt32(calcBusinessDays);

            var isAuthorised = await _authorizationService.AuthorizeAsync(
                User, @event, EventsOperations.Create);
            if(isAuthorised.Succeeded == false)
            {
                return Forbid();
            }
            _context.Add(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.events == null)
            {
                return NotFound();
            }

            var @event = await _context.events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            var isAuthorised = await _authorizationService.AuthorizeAsync(
                User, @event, EventsOperations.Update);
            if (isAuthorised.Succeeded == false)
                return Forbid();

            if (@event.Status == EventStatus.Approved || @event.Status == EventStatus.Rejected)
                return new ObjectResult("Event is eaither Approved or Rejected and cannot be edited.") { StatusCode = 403 };

            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,StartDate,EndDate,CreatorId")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            try
            {
                @event.CreatorId = _userManager.GetUserId(User);

                //calculate working days except bank holidays
                double calcBusinessDays =
                1 + ((@event.EndDate - @event.StartDate).TotalDays * 5 -
                (@event.StartDate.DayOfWeek - @event.EndDate.DayOfWeek) * 2) / 7;

                if (@event.EndDate.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
                if (@event.StartDate.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

                @event.Days = Convert.ToInt32(calcBusinessDays);

                _context.Update(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.Id))
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

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.events == null)
            {
                return NotFound();
            }

            var @event = await _context.events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            var isAuthorised = await _authorizationService.AuthorizeAsync(
                User, @event, EventsOperations.Delete);
            if (isAuthorised.Succeeded == false)
                return Forbid();

            if (@event.Status == EventStatus.Approved || @event.Status == EventStatus.Rejected)
                return new ObjectResult("Event is eaither Approved or Rejected and cannot be edited.") { StatusCode = 403 };

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.events == null)
            {
                return Problem("Entity set 'ApplicationDbContext.events'  is null.");
            }
            var @event = await _context.events.FindAsync(id);

            var isAuthorised = await _authorizationService.AuthorizeAsync(
                User, @event, EventsOperations.Delete);
            if (isAuthorised.Succeeded == false)
                return Forbid();

            if (@event != null)
            {
                _context.events.Remove(@event);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
          return (_context.events?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public void Pagination(List<Event> eventsList)
        {

        }
    }
}
