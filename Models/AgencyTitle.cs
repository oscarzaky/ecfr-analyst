using System.Text.Json.Serialization;

namespace eCFR.Analyst.Models
{
    public class AgencyTitleResponse
    {
        public List<AgencyTitle> Titles { get; set; } = new List<AgencyTitle>();
    }
 
    public class AgencyTitle
    {
        public virtual Agency Agency { get; set; } = new Agency();
        public int Id { get; set; } = 0;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("chapter")]
        public string Chapter { get; set; } = string.Empty;
    }
}