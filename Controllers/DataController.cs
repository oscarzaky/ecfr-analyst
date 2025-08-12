using eCFR.Analyst.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace eCFR.Analyst.Controllers
{
    public class DataController : Controller
    {
        private readonly IEcfrService _ecfrService;

        public DataController(IEcfrService ecfrService)
        {
            _ecfrService = ecfrService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FetchData()
        {
            try
            {
                // For this project, we'll fetch a specific, relevant title as an example.
                // Title 42: Public Health is a good example due to its size and relevance.
                await _ecfrService.FetchAndStoreTitleData(42);
                TempData["Message"] = "Successfully fetched and stored data for Title 42.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while fetching data: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}