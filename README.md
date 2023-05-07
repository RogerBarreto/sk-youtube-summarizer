# Summarize Youtube Videos Quickly

![summarizer-demo](https://user-images.githubusercontent.com/19890735/236650648-c1cb2791-6c3c-440e-ad9e-3ec08a349de9.gif)

Summarize your Youtube Videos quickly using your preferred language.

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
