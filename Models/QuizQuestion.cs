using Newtonsoft.Json;

namespace ExcursionTextbook.Models;

public class QuizQuestion
{
    [JsonProperty("question")]
    public string Question { get; set; } = string.Empty;

    [JsonProperty("options")]
    public List<string> Options { get; set; } = new();

    [JsonProperty("correctIndex")]
    public int CorrectIndex { get; set; }
}
