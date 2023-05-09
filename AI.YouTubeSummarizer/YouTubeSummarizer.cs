using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using System.Text;
using YoutubeTranscriptApi;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace AI.YouTubeSummarizer;

public class YouTubeSummarizer
{
    private ConfigurationSettings _configData;
    private Queue<TranscriptItem> _transcriptItems;
    private StringBuilder _sb;
    private double _lastStartTime;
    private TranscriptItem _lastItem;
    private string _videoTitle;
    private string _selectedLanguage;

    public YouTubeSummarizer(ConfigurationSettings configData)
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

        var chat = (OpenAIChatHistory)chatCompletion.CreateNewChat("You are a summarizer, I will provide you any language truncated text and you are going to summarize it for me in " + this._selectedLanguage);

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

            foreach (var item in transcriptResult.FindTranscriptOrDefaultToExisting("en").Fetch())
            {
                this._transcriptItems.Enqueue(item);
            }
        }
    }

    private async Task ProcessTranscriptItemsAsync(OpenAIChatHistory chat, IChatCompletion chatCompletion)
    {
        this._lastStartTime = 0;
        do
        {
            this._lastItem = this._transcriptItems.Dequeue();

            this._sb.Append(this._lastItem.Text);

            if (this._sb.Length > 2000)
            {
                await ProcessChatMessage(chat, chatCompletion);
            }
        }
        while (this._transcriptItems.Count > 0);

        await ProcessChatMessage(chat, chatCompletion);
    }

    private async Task ProcessChatMessage(OpenAIChatHistory chat, IChatCompletion chatCompletion)
    {
        PrintProcessingTime();

        if (chat.Messages.Count > 1)
        {
            chat.Messages.RemoveRange(1, chat.Messages.Count - 1);
        }

        chat.AddUserMessage(this._sb.ToString());
        var assistantMessage = await TryResilientlyAsync(chat, chatCompletion);
        chat.AddAssistantMessage(assistantMessage);

        this._sb.Clear();
        this._lastStartTime = this._lastItem.Start;
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
                var assistantMessage = string.Empty;

                await foreach (string assistantWord in chatCompletion.GenerateMessageStreamAsync(chat,
                                   new ChatRequestSettings { MaxTokens = 1000, Temperature = 1 }))
                {
                    assistantMessage += assistantWord;
                    Console.Write(assistantWord);
                }
                Console.Write("\n");

                return assistantMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}... trying again in 3 seconds");
                await Task.Delay(3000);
            }
        } while (trials < 5);

        throw new Exception("Failed more times than expected. Auto Aborted.");
    }

    /// <summary>
    /// This method is going to prompt the user for the languages to be used in the summarization only if the languages has more than one option.
    /// </summary>
    internal void PromptForLanguages()
    {
        if (this._configData.Languages is null || this._configData.Languages.Count == 0)
        {
            Console.WriteLine("⚠️ No language configuration detected.\n\nDefaulting to English.\n");
            this._selectedLanguage = "English";

            return;
        }

        if (this._configData.Languages.Count > 1)
        {
            Console.WriteLine("Please select the language you want to use for the summarization: ");
            for (int i = 0; i < this._configData.Languages.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {this._configData.Languages[i]}");
            }
            Console.Write("Option: ");

            do 
            { 
                var option = Console.ReadLine();
                if (int.TryParse(option, out int optionNumber))
                {
                    if (optionNumber > 0 && optionNumber <= this._configData.Languages.Count)
                    {
                        this._selectedLanguage = this._configData.Languages[optionNumber - 1];

                        Console.WriteLine($"\nSelected language: {this._selectedLanguage}\n");
                        return;
                    }
                }

                Console.Write("\nInvalid option, please try again.\nOption: ");
            } while (this._selectedLanguage is null);
        }

        this._selectedLanguage = this._configData.Languages[0];
        Console.WriteLine($"ℹ️ Only one language configuration detected.\n\nAuto-Selected language: {this._selectedLanguage}\n");
        return;
    }
}
