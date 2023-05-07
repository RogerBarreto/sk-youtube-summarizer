using System.Text.Json.Serialization;

namespace AI.YouTubeSummarizer;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class ConfigurationSettings
{
    public string OPENAI_KEY { get; set; }

    [JsonPropertyName("languages")]
    public IReadOnlyList<string> Languages { get; set; }
}
