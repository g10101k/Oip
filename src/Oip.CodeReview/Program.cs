using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using OllamaSharp;

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
        var config = BindConfig(args);

        var ollamaApiClient = new OllamaApiClient(new Uri(config.Ollama.Url));
        ollamaApiClient.SelectedModel = config.Ollama.Model;

        GitFetchBranch(config.WorkDir, config.SourceBranch);

        var diff = GetDiffUsingGitCli(config.WorkDir, config.SourceBranch, config.TargetBranch, config.FilePath);

        var promptFormat = await File.ReadAllTextAsync("prompt.txt");

        var prompt = string.Format(promptFormat, diff);
        if (config.PromptOnly)
        {
            Console.WriteLine(prompt);
        }
        else
        {
            await foreach (var stream in ollamaApiClient.GenerateAsync(prompt))
                Console.Write(stream?.Response);
        }
    }

    /// <summary>
    /// Retrieves the diff between two branches using the Git command-line interface
    /// </summary>
    /// <param name="repoPath">The path to the Git repository</param>
    /// <param name="sourceBranch">The source branch</param>
    /// <param name="targetBranch">The target branch</param>
    /// <param name="filePath">Optional path to specific file for diff comparison</param>
    /// <returns>The diff output as a string</returns>
    private static string GetDiffUsingGitCli(string repoPath, string sourceBranch, string targetBranch,
        string? filePath = null)
    {
        return GitProcessStartInfo(repoPath,
            $"diff -w --inter-hunk-context=100 --ignore-all-space {targetBranch}...{sourceBranch} {filePath}".Trim());
    }

    private static void GitFetchBranch(string repoPath, string targetBranch)
    {
        GitProcessStartInfo(repoPath, "fetch origin");
        GitProcessStartInfo(repoPath, $"checkout {targetBranch}");
    }

    /// <summary>
    /// Executes a Git command and returns the output.
    /// </summary>
    /// <param name="repoPath">The path to the Git repository.</param>
    /// <param name="arguments">The arguments to pass to the Git command.</param>
    /// <returns>The output of the Git command.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Git command fails.</exception>
    private static string GitProcessStartInfo(string repoPath, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed: {error}");
        }

        return output;
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