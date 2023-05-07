# Summarize Youtube Videos Quickly

Summarize your Youtube Videos quickly using your preferred language.

## End user - Installation

Get the released version executable. Here:

1. Setup your OpenAI API Key

   Create an environment variable named `OPENAI_KEY` with your OpenAI API Key.

## Developer Setup

### Secrets

If you already have an environment variable named `OPENAI_KEY` you can skip this step as it will override the user secret value.

⚠️ Use .NET [Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to avoid the risk of leaking secrets into the repository, branches and pull requests.

⚠️ If you have not used the Secret Manager tool before, you must first initialize the user secrets store in the project directory. Run the following command from the project directory:

```powershell
dotnet user-secrets init
dotnet user-secrets set "OPENAI_KEY" "<your key>"
```

### Running / Development

```powershell
dotnet run
```

### Deploying / Publishing

```powershell
dotnet publish -c Release
```

## Projects Used

- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) (Package)
- [YouTube Transcript API Sharp](https://github.com/BobLd/youtube-transcript-api-sharp) (Modified)
