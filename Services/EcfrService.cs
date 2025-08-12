using eCFR.Analyst.Models;
using eCFR.Analyst.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace eCFR.Analyst.Services
{
    public class EcfrService : IEcfrService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;

        public EcfrService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task FetchAndStoreTitleData(int titleNumber)
        {
            // Implementation placeholder
            await Task.CompletedTask;
        }

        public async Task<List<Agency>> GetAgenciesAsync()
        {
            List<Agency> agencies = new List<Agency>();
            // Check if agencies exist in database
            var existingAgencies = await _context.Agencies.ToListAsync();
            if (existingAgencies.Any())
            {
                return existingAgencies;
            }

            // Fetch from API and save to database
            var response = await _httpClient.GetStringAsync("admin/v1/agencies.json");
            var apiResponse = JsonSerializer.Deserialize<AgencyResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Agencies != null)
            {
                foreach (var apiAgency in apiResponse.Agencies)
                {
                    var agency = new Agency
                    {
                        ApiId = apiAgency.ApiId,
                        Name = apiAgency.Name,
                        Slug = apiAgency.Slug,
                        Short_Name = apiAgency.Short_Name ?? string.Empty,
                        Cfr_References = JsonSerializer.Serialize(apiAgency.Cfr_References),
                        AgencyDetail = JsonSerializer.Serialize(apiAgency)
                    };
                    if (!await _context.Agencies.AnyAsync(a => a.Name == agency.Name))
                    {
                        _context.Agencies.Add(agency);
                    }
                    await SaveTitles(agency);
                }
                await _context.SaveChangesAsync();
                agencies = await _context.Agencies.ToListAsync();
            }

            return agencies;
        }

        public async Task<List<AgencyTitle>> GetTitlesAsync(int? agencyId = null)
        {
            var titles = await _context.Titles.Include(t => t.Agency).ToListAsync();
            if (agencyId.HasValue)
            {
                titles = titles.Where(t => t.Agency.Id == agencyId.Value)
                               .Select(t => new AgencyTitle
                               {
                                   Id = t.Id,
                                   Title = t.Title,
                                   Chapter = t.Chapter,
                                   Agency = t.Agency
                               })
                               .ToList();
            }
            if (titles.Any())
            {
                return titles;
            }

            // Optionally, you could fetch from an API and populate the database here if needed.
            // For now, just return the list from the database.
            return new List<AgencyTitle>();
        }
        public async Task<Agency?> GetAgencyAsync(string agencyId)
        {
            if (!int.TryParse(agencyId, out int id))
            {
                return null;
            }

            return await _context.Agencies.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<object>> GetAgencyTitlesAsync(int agencyId)
        {
            try
            {
                // Get agency from database
                // if (!int.TryParse(agencyId, out int id))
                // {
                //     throw new ArgumentException($"Invalid agency ID: {agencyId}");
                // }

                // var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId);
                var titles = await _context.Titles.Include(t => t.Agency).Where(t => t.Agency.Id == agencyId).ToListAsync();
                if (titles == null || !titles.Any())
                {
                    throw new ArgumentException($"Agency not found with ID: {agencyId}");
                }

                if (string.IsNullOrEmpty(titles.First().Agency.AgencyDetail))
                {
                    throw new InvalidOperationException($"Agency {titles.First().Agency.Name} has no detail data");
                }

                // Parse Cfr_References JSON to extract titles
                var agencyTitles = JsonSerializer.Deserialize<List<ApiAgencyTitle>>(titles.First().Agency.Cfr_References, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false
                });

                return agencyTitles?.Select(t => new { id = t.Id, title = $"Title {t.Title}, Chapter {t.Chapter}" }).Cast<object>().ToList() ?? new List<object>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse agency detail JSON: {ex.Message}", ex);
            }
        }

        public async Task<ApiAgencyTitle> GetSingleAgencyTitleAsync(int agencyId)
        {
            try
            {
                // Get agency from database
                // if (!int.TryParse(agencyId, out int id))
                // {
                //     throw new ArgumentException($"Invalid agency ID: {agencyId}");
                // }

                var title = await _context.Titles.Include(t => t.Agency).FirstOrDefaultAsync(a => a.Agency.Id == agencyId);
                if (title == null)
                {
                    throw new ArgumentException($"Title not found with agency ID: {agencyId}");
                }

                if (string.IsNullOrEmpty(title.Agency.AgencyDetail))
                {
                    throw new InvalidOperationException($"Agency {title.Agency.Name} has no detail data");
                }

                // Parse Cfr_References JSON to extract titles
                var agencyTitles = JsonSerializer.Deserialize<List<ApiAgencyTitle>>(title.Agency.Cfr_References, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false
                });

                return agencyTitles?.FirstOrDefault();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse agency detail JSON: {ex.Message}", ex);
            }
        }
        public async Task<string> SaveTitles(Agency agency)
        {
            List<AgencyTitle> agencyTitles = new List<AgencyTitle>();
            var apiTitles = JsonSerializer.Deserialize<List<ApiAgencyTitle>>(agency.Cfr_References);
            foreach (var apiTitle in apiTitles)
            {
                // Check if title already exists
                var existingTitle = await _context.Titles
                    .FirstOrDefaultAsync(t => t.Title == apiTitle.Title.ToString() && t.Chapter == apiTitle.Chapter && t.Agency.Id == agency.Id);

                if (existingTitle != null)
                {
                    continue; // Skip if title already exists
                }

                var title = new AgencyTitle
                {
                    Agency = agency,
                    Title = apiTitle.Title.ToString(),
                    Chapter = apiTitle.Chapter
                };
                _context.Titles.Add(title);
            }
            await _context.SaveChangesAsync();

            return agencyTitles.Count > 0 ? "Titles saved successfully" : "No new titles to save";
        }   

        public async Task<int> CountWords(string titleNumber)
        {
            try
            {
                var responseCount = await _httpClient.GetStringAsync($"search/v1/counts/titles");
                var countResponse = JsonSerializer.Deserialize<TitleCountResponse>(responseCount, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (countResponse?.Titles != null && countResponse.Titles.ContainsKey(titleNumber))
                {
                    return countResponse.Titles[titleNumber];
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<CorrectionsResponse>> GetHistory(string text)
        {
            List<CorrectionsResponse> corrections = new List<CorrectionsResponse>();
            try
            {
                // Fetch title data from eCFR API
                var result = await _httpClient.GetStringAsync($"admin/v1/corrections.json?title={text}");

                var response = JsonSerializer.Deserialize<EcfrCorrectionResponse>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false
                });

                if (response != null)
                {
                    // Sort corrections by LastModified date in descending order
                    corrections = response.EcfrCorrections
                        .OrderByDescending(c => c.LastModified)
                        .Select(c => new CorrectionsResponse
                        {
                            Id = c.Id,
                            CfrReferences = c.CfrReferences,
                            CorrectiveAction = c.CorrectiveAction,
                            ErrorCorrected = c.ErrorCorrected,
                            ErrorOccurred = c.ErrorOccurred,
                            FrCitation = c.FrCitation,
                            Position = c.Position,
                            DisplayInToc = c.DisplayInToc,
                            Title = c.Title,
                            Year = c.Year,
                            LastModified = c.LastModified
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return corrections;
            
        }

        public string CalculateChecksum(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToHexString(hash);
            }
        }

        public double CalculateComplexityScore(int wordCount)
        {
            return Math.Round(wordCount / 1000.0, 2);
        }
    }

    public class AgencyResponse
    {
        public List<ApiAgency> Agencies { get; set; } = new List<ApiAgency>();
    }
    public class AgencyTitleData
    {
        public List<AgencyTitle> Titles { get; set; } = new List<AgencyTitle>();
    }

    public class ApiAgency
    {
        public string Id { get; set; } = string.Empty;
        public string ApiId => Id;
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
        [JsonPropertyName("short_name")]
        public string Short_Name { get; set; } = string.Empty;
        [JsonPropertyName("sortable_name")]
        public string Sortable_Name { get; set; } = string.Empty;
        [JsonPropertyName("cfr_references")]
        public JsonElement Cfr_References { get; set; }
    }

    public class AgencyTitleResponse
    {
        public List<ApiAgencyTitle> Titles { get; set; } = new List<ApiAgencyTitle>();
    }

    public class TitlesResponse
    {
        public List<ApiTitle> Titles { get; set; } = new List<ApiTitle>();
    }

    public class ApiAgencyTitle
    {
        public int Id => Title;
        [JsonPropertyName("title")]
        public int Title { get; set; } = 0;
        [JsonPropertyName("chapter")]
        public string Chapter { get; set; } = string.Empty;
    }

    public class ApiTitle
    {
        public int Title { get; set; }
        public string Chapter { get; set; } = string.Empty;
    }
    public class EcfrCorrectionResponse
    {
        [JsonPropertyName("ecfr_corrections")]
        public List<CorrectionsResponse> EcfrCorrections { get; set; } = new List<CorrectionsResponse>();

    }


    public class CorrectionsResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("cfr_references")]
        public List<EcfrCorrectionItem> CfrReferences { get; set; } = new List<EcfrCorrectionItem>();
        // [JsonPropertyName("ecfr_corrections")]
        // public List<EcfrCorrectionResponse> EcfrCorrections { get; set; } = new List<EcfrCorrectionResponse>();
        [JsonPropertyName("corrective_action")]
        public string CorrectiveAction { get; set; } = string.Empty;
        [JsonPropertyName("error_corrected")]
        public string ErrorCorrected { get; set; } = string.Empty;
        [JsonPropertyName("error_occurred")]
        public string ErrorOccurred { get; set; } = string.Empty;
        [JsonPropertyName("fr_citation")]
        public string FrCitation { get; set; } = string.Empty;
        [JsonPropertyName("position")]
        public int Position { get; set; }
        [JsonPropertyName("display_in_toc")]
        public bool DisplayInToc { get; set; }
        [JsonPropertyName("title")]
        public int Title { get; set; }
        [JsonPropertyName("year")]
        public int Year { get; set; }
        [JsonPropertyName("last_modified")]
        public DateTime LastModified { get; set; }    }

    public class TitleCountResponse
    {
        [JsonPropertyName("titles")]
        public Dictionary<string, int> Titles { get; set; } = new Dictionary<string, int>();
    }

    public class EcfrReference
    {
        [JsonPropertyName("titles")]
        public string Titles { get; set; } = string.Empty;
        [JsonPropertyName("cfr_reference")]
        public List<EcfrCorrectionItem> CfrReference { get; set; } = new List<EcfrCorrectionItem>();
        [JsonPropertyName("hierarchy")]
        public List<string> Hierarchy { get; set; } = new List<string>();
    }
    public class EcfrCorrectionItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("chapter")]
        public string Chapter { get; set; } = string.Empty;
        [JsonPropertyName("part")]
        public string Part { get; set; } = string.Empty;
        [JsonPropertyName("subpart")]
        public string Subpart { get; set; } = string.Empty;
        [JsonPropertyName("section")]
        public string Section { get; set; } = string.Empty;
    }

}