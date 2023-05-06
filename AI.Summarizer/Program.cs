using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using System.Text;
using YoutubeTranscriptApi;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AI.YouTubeSummarizer;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IConfiguration configuration = ConfigureSettings();
        var configData = configuration.Get<ConfigurationData>();

        var summarizer = new YouTubeSummarizer(configData);

        Console.WriteLine("======== Welcome to YouTube video Summarizer =========\n");
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
        var builder = new ConfigurationBuilder().AddUserSecrets<ConfigurationData>();
        return builder.Build();
    }
}

public class YouTubeSummarizer
{
    private ConfigurationData _configData;
    private Queue<TranscriptItem> _transcriptItems;
    private StringBuilder _sb;
    private double _lastStartTime;
    private TranscriptItem _lastItem;
    private string _videoTitle;

    public YouTubeSummarizer(ConfigurationData configData)
    {
        this._configData = configData;
        this._transcriptItems = new Queue<TranscriptItem>();
        this._sb = new StringBuilder();
    }

    public async Task SummarizeVideoAsync(string youTubeUrl)
    {
        var videoId = GetVideoId(youTubeUrl);
        if (videoId is null)
        {
            throw new ArgumentNullException(nameof(videoId));
        }

        this.FetchTranscriptItems(videoId);

        Console.WriteLine(@$"
--------------------------------------------------
Title: {this._videoTitle}
--------------------------------------------------");
        IChatCompletion chatCompletion = new OpenAIChatCompletion("gpt-3.5-turbo", this._configData.OPENAI_KEY);

        var chat = (OpenAIChatHistory)chatCompletion.CreateNewChat("You are a summarizer, every text I provide you are going to summarize it for me");

        if (this._transcriptItems.Count > 0)
        {
            await ProcessTranscriptItemsAsync(chat, chatCompletion);
        }

        Console.WriteLine("\n=== End of Summarization ===\n");
    }

    private string? GetVideoId(string youTubeUrl)
    {
        return youTubeUrl?.Replace("https://www.youtube.com/watch?v=", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }

    private void FetchTranscriptItems(string videoId)
    {
        using (var youTubeTranscriptApi = new YouTubeTranscriptApi())
        {
            TranscriptList transcriptResult = youTubeTranscriptApi.ListTranscripts(videoId);
            this._videoTitle = transcriptResult.VideoTitle;

            foreach (var item in transcriptResult.FindTranscript(new List<string> { "en" }).Fetch())
            {
                this._transcriptItems.Enqueue(item);
            }
        }
    }

    private async Task ProcessTranscriptItemsAsync(OpenAIChatHistory chat, IChatCompletion chatCompletion)
    {
        do
        {
            this._lastItem = this._transcriptItems.Dequeue();

            this._sb.Append(this._lastItem.Text);

            if (this._sb.Length > 2000)
            {
                PrintProcessingTime();

                if (chat.Messages.Count > 1)
                {
                    chat.Messages.RemoveRange(1, chat.Messages.Count - 1);
                }

                chat.AddUserMessage(this._sb.ToString());
                var assistantMessage = await TryResilientlyAsync(chat, chatCompletion);
                chat.AddAssistantMessage(assistantMessage);

                Console.WriteLine(assistantMessage);
                this._sb.Clear();
                this._lastStartTime = this._lastItem.Start;
            }
        }
        while (this._transcriptItems.Count > 0);

        PrintProcessingTime();

        chat.AddUserMessage(this._sb.ToString());
        Console.WriteLine(await chatCompletion.GenerateMessageAsync(chat, new ChatRequestSettings { MaxTokens = 1000, Temperature = 1 }));
    }

    private void PrintProcessingTime()
    {
        Console.Write($"\nFrom: {TimeSpan.FromSeconds(this._lastStartTime).Hours:D2}:{TimeSpan.FromSeconds(this._lastStartTime).Minutes:D2}:{TimeSpan.FromSeconds(this._lastStartTime).Seconds:D2} --> ");
        Console.WriteLine($"{TimeSpan.FromSeconds(this._lastItem.Start).Hours:D2}:{TimeSpan.FromSeconds(this._lastItem.Start).Minutes:D2}:{TimeSpan.FromSeconds(this._lastItem.Start).Seconds:D2} ... Processing\n");
    }

    private static async Task<string> TryResilientlyAsync(OpenAIChatHistory chat, IChatCompletion chatCompletion)
    {
        int trials = 0;
        do
        {
            try
            {
                trials++;
                return await chatCompletion.GenerateMessageAsync(chat, new ChatRequestSettings { MaxTokens = 1000, Temperature = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}... trying again in 3 seconds");
                await Task.Delay(3000);
            }
        } while (trials < 5);

        throw new Exception("Failed more times than expected. Auto Aborted.");
    }
}
