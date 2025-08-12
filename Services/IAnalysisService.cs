namespace eCFR.Analyst.Services
{
    public interface IAnalysisService
    {
        Task<object?> AnalyzeTitleAsync(string titleDate, int titleNumber);


    }
}