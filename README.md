# Summarize Youtube Videos Quickly

Summarize your Youtube Videos Quickly

## Installation

Use .NET [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to avoid the risk of leaking secrets into the repository, branches and pull requests.

```powershell
cd AI.Summarizer

dotnet user-secrets init
dotnet user-secrets set "OPENAI_KEY" "<your key>"

dotnet run
```

## Projects Dependencies

- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [YouTube Transcript API Sharp](https://github.com/BobLd/youtube-transcript-api-sharp)
