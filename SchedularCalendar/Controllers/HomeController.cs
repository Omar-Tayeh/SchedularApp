using Microsoft.AspNetCore.Mvc;
using SchedularCalendar.Data;
using SchedularCalendar.Helpers;
using SchedularCalendar.Models;
using System.Diagnostics;

namespace SchedularCalendar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var approvedEvents = _context.events.Where(e => e.Status == EventStatus.Approved).ToList();

            ViewData["Events"] = JSONHelper.GetEventListJSONString(approvedEvents);
            return View(approvedEvents);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}