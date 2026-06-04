using Newtonsoft.Json;

namespace ExcursionTextbook.Models;

public class Section
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;

    [JsonProperty("imagePath")]
    public string? ImagePath { get; set; }
}
