using eCFR.Analyst.Models;

namespace eCFR.Analyst.Services
{
    public interface IEcfrService
    {
        Task FetchAndStoreTitleData(int titleNumber);
        Task<List<Agency>> GetAgenciesAsync();
        Task<List<AgencyTitle>> GetTitlesAsync(int? agencyId = null);
        Task<Agency?> GetAgencyAsync(string agencyId);
        Task<List<object>> GetAgencyTitlesAsync(int agencyId);

        Task<ApiAgencyTitle> GetSingleAgencyTitleAsync(int agencyId);

        Task<int> CountWords(string text);

        Task<List<CorrectionsResponse>> GetHistory(string text);

        string CalculateChecksum(string text);

        double CalculateComplexityScore(int wordCount);
        
    }
}