using Microsoft.Extensions.Configuration;
using System.Text;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AI.YouTubeSummarizer;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        IConfiguration configuration = ConfigureSettings();
        var configData = configuration.Get<ConfigurationData>();

        var summarizer = new YouTubeSummarizer(configData);

        Console.WriteLine("======== Welcome to YouTube video Summarizer =========\n");

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

    private static IConfiguration ConfigureSettings()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<ConfigurationData>()
            .AddJsonFile("config.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
