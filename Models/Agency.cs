namespace eCFR.Analyst.Models
{
    public class Agency
    {
        public int Id { get; set; }
        public string ApiId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Short_Name { get; set; } = string.Empty;
        public string? Cfr_References { get; set; } = string.Empty;
        public string AgencyDetail { get; set; } = string.Empty;
    }
}