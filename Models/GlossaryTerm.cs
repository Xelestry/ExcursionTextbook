using Newtonsoft.Json;

namespace ExcursionTextbook.Models;

public class GlossaryTerm
{
    [JsonProperty("term")]
    public string Term { get; set; } = string.Empty;

    [JsonProperty("definition")]
    public string Definition { get; set; } = string.Empty;
}
