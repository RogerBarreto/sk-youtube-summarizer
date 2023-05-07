using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AI.YouTubeSummarizer;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("======== Welcome to YouTube video Summarizer =========\n");

            ConfigurationSettings configData = ConfigureSettings();
            var summarizer = new YouTubeSummarizer(configData);


            summarizer.PromptForLanguages();
            do
            {
                Console.WriteLine("YouTube Video URL: (https://www.youtube.com/watch?v=...)");
                var youTubeUrl = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(youTubeUrl))
                {
                    continue;
                }

                try
                {
                    await summarizer.SummarizeVideoAsync(youTubeUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            while (true);
        }
        catch (EndException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static ConfigurationSettings ConfigureSettings()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<ConfigurationSettings>()
            .AddJsonFile("config.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var configData = configuration.Get<ConfigurationSettings>();
        configData.OPENAI_KEY = GetApiKey(configData);

        return configData;
    }

    private static string GetApiKey(ConfigurationSettings configData)
    {
        const string openAIKey = "OPENAI_KEY";

        var apiKey =
            Environment.GetEnvironmentVariable(openAIKey) ??
            Environment.GetEnvironmentVariable(openAIKey, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(openAIKey, EnvironmentVariableTarget.Machine);

        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = configData.OPENAI_KEY;
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new EndException("⛔ OPENAI_KEY is not set\n\nConfigure it using either Environment Variables or User Secrets.\n\nPlease refer to installation instructions here: https://github.com/RogerBarreto/sk-youtube-summarizer#readme");
        }

        return apiKey;
    }
}
