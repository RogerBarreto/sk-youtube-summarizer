using System.Text.Json.Serialization;

namespace AI.YouTubeSummarizer;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class ConfigurationData
{
    public string OPENAI_KEY { get; set; }

    [JsonPropertyName("languages")]
    public IReadOnlyList<LanguageConfig> Languages { get; set; }
}

public class LanguageConfig
{
    [JsonPropertyName("alias")]
    public string Alias { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
}
