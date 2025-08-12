using eCFR.Analyst.Data;
using eCFR.Analyst.Models;
using eCFR.Analyst.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace eCFR.Analyst.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IEcfrService _ecfrService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IEcfrService ecfrService)
        {
            _logger = logger;
            _context = context;
            _ecfrService = ecfrService;
        }

        public async Task<IActionResult> Index()
        {
            // var titles = _context.Titles
            //     .Select(t => new SelectListItem
            //     {
            //         Value = t.Title.ToString(),
            //         Text = "Title " + t.Title.ToString() + " - " + t.Chapter
            //     })
            //     .ToList();

            var agencies = await _ecfrService.GetAgenciesAsync();
            var titles = await _ecfrService.GetTitlesAsync();
            var agencyList = agencies.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();

            ViewBag.Titles = titles.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"Title {t.Title}, Chapter {t.Chapter}"
            }).ToList();
            ViewBag.Agencies = agencyList;
            return View();
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