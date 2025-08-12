using eCFR.Analyst.Models;
using eCFR.Analyst.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace eCFR.Analyst.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly IEcfrService _eCfrService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(IAnalysisService analysisService, IEcfrService eCfrService, HttpClient httpClient, ILogger<AnalysisController> logger)
        {
            _analysisService = analysisService;
            _eCfrService = eCfrService;
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("{titleId}")]
        public async Task<IActionResult> AnalyzeTitle(int titleId)
        {
            try
            {
                var agencyTitles = await _eCfrService.GetTitlesAsync();
                var agencyTitle = agencyTitles.FirstOrDefault(t => t.Id == titleId);
                // Calculate word count
                var wordCount = agencyTitle != null ? await _eCfrService.CountWords(agencyTitle.Title.ToString()) : 0;
                // Historical
                var historical = agencyTitle != null ? await _eCfrService.GetHistory(agencyTitle.Title.ToString()) : null;
                // if (historical != null && historical.Any())
                // {
                //     historical = historical.OrderByDescending(c => DateTime.TryParse(c.CfrReference.ElementAtOrDefault(0)?.LastModified.ToString(), out var date) ? date : DateTime.MinValue).ToList();
                // }

                // Calculate checksum
                var checksum = agencyTitle != null ? _eCfrService.CalculateChecksum(agencyTitle.Title.ToString()) : null;
                //var checksum = new List<string>();

                var retVal = (new
                {
                    titleNumber = agencyTitle?.Title,
                    totalWordCount = wordCount,
                    corrections = historical,
                    aggregateChecksum = checksum,
                    regulatoryComplexityScore = _eCfrService.CalculateComplexityScore(wordCount),
                });

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing title {titleId}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}