using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Oip.CodeReview;

/// <summary>
/// Contains the entry point for the code review application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Contains the entry point for the code review application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static async Task Main(string[] args)
    {
        try
        {
            var config = BindConfig(args);

            var diff = GitHelper.GetDiffUsingGitCli(config.WorkDir, config.SourceBranch, config.TargetBranch,
                config.ExcludeFolders, config.FilePath, config.NewCodeOnly);

            var systemMessage = await File.ReadAllTextAsync("prompt.txt");
            var openAiApiSettings = config.OpenAiApiSettings;
            if (!config.PromptOnly && openAiApiSettings != null)
            {
                if (string.IsNullOrEmpty(openAiApiSettings.BaseUrl))
                    throw new InvalidOperationException("BaseUrl is not set");
                if (string.IsNullOrEmpty(openAiApiSettings.ModelId))
                    throw new InvalidOperationException("ModelId is not set");

                var builder = Kernel.CreateBuilder();
                builder.Services.AddHttpClient();
                builder.Services.AddOpenAIChatCompletion(openAiApiSettings.ModelId, new Uri(openAiApiSettings.BaseUrl),
                    openAiApiSettings.ApiKey);
                var kernel = builder.Build();

                var history = new ChatHistory();
                history.AddSystemMessage(systemMessage);
                history.AddUserMessage(diff);

                var chat = kernel.GetRequiredService<IChatCompletionService>();
                var result = await chat.GetChatMessageContentAsync(history, kernel: kernel);

                if (result.Content is null)
                    throw new InvalidOperationException("Content is null");

                Console.WriteLine(result.Content);
            }
            else
            {
                Console.WriteLine(systemMessage);
                Console.WriteLine(diff);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    /// <summary>
    /// Binds configuration settings from various sources
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>The configured application settings</returns>
    private static AppSettings BindConfig(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<AppSettings>()
            .AddCommandLine(args)
            .Build();

        var appConfig = new AppSettings();
        configuration.Bind(appConfig);
        return appConfig;
    }
}