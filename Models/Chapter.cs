using Newtonsoft.Json;

namespace ExcursionTextbook.Models;

public class Chapter
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("sections")]
    public List<Section> Sections { get; set; } = new();

    [JsonProperty("quiz")]
    public List<QuizQuestion> Quiz { get; set; } = new();

    [JsonProperty("glossaryTerms")]
    public List<GlossaryTerm> GlossaryTerms { get; set; } = new();
}
