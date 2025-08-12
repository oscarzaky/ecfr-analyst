using eCFR.Analyst.Services;
using Microsoft.AspNetCore.Mvc;

namespace eCFR.Analyst.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly IEcfrService _ecfrService;
        private readonly ILogger<AgencyController> _logger;

        public AgencyController(IEcfrService ecfrService, ILogger<AgencyController> logger)
        {
            _ecfrService = ecfrService;
            _logger = logger;
        }

        [HttpGet("{agencyId}")]
        public async Task<IActionResult> GetAgency(string agencyId)
        {
            try
            {
                if (string.IsNullOrEmpty(agencyId))
                {
                    return BadRequest("Agency ID is required");
                }

                var agency = await _ecfrService.GetAgencyAsync(agencyId);
                if (agency == null)
                {
                    return NotFound($"Agency with ID {agencyId} not found");
                }

                return Ok(new { 
                    id = agency.Id,
                    name = agency.Name,
                    short_Name = agency.Short_Name,
                    cfr_References = agency.Cfr_References
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting agency {agencyId}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("{agencyId}/titles")]
        public async Task<IActionResult> GetAgencyTitles(int agencyId)
        {
            try
            {
                // if (string.IsNullOrEmpty(agencyId))
                // {
                //     return BadRequest("Agency ID is required");
                // }

                _logger.LogInformation($"Getting titles for agency ID: {agencyId}");
                var titles = await _ecfrService.GetTitlesAsync(agencyId);
                return Ok(titles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting titles for agency {agencyId}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}