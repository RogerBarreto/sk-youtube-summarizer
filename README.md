# Summarize Youtube Videos Quickly for You

This project has the simple purpose of make it very easy and fast to summarize youtube video transcriptions in your preferred language and platform (Windows or Linux)

Windows -> Portuguese Summarization:

![summarizer-win-portuguese](https://user-images.githubusercontent.com/19890735/236685352-3e31e223-8fe7-4823-8d29-d2accf5fea17.gif)

Linux -> English Summarization: 

![summarizer-linux-english](https://user-images.githubusercontent.com/19890735/236685356-5b83e37f-5e9e-4d51-b3bb-0799d2594cea.gif)

## End user - Setup

Get the released version executable. Here:

##### Setup your OpenAI API Key

Create an environment variable named `OPENAI_KEY` with your OpenAI API Key.

##### Optionally you can use your own preferred languages to summarize

Change `config.json` file with your preferred languages and leave it in the same folder as the application.

```json
{
  "languages": ["English", "Your Language 1", "Your Language 2", ...]
}
```

## Developer Setup

### Secrets

If you already have an environment variable named `OPENAI_KEY` you can skip this step as it will override the user secret value.

⚠️ Use .NET [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to avoid the risk of leaking secrets into the repository, branches and pull requests.

⚠️ If you have not used the Secret Manager tool before, you must first initialize the user secrets store in the project directory. Run the following command from the project directory:

```powershell
dotnet user-secrets init
dotnet user-secrets set "OPENAI_KEY" "<your key>"
```

Running / Debugging

```powershell
dotnet run
```

### Deploying / Publishing

Windows:

```powershell
dotnet publish -r win-x64 -c Release
```

Linux:

```powershell
dotnet publish -r linux-x64 -c Release -p:PublishReadyToRun=true
```

## Projects Used

- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) (Package)
- [YouTube Transcript API Sharp](https://github.com/BobLd/youtube-transcript-api-sharp) (Modified)
