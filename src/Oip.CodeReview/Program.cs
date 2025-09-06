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

        OllamaApiClient ollamaApiClient = new OllamaApiClient(new Uri(config.Ollama.Url));
        ollamaApiClient.SelectedModel = config.Ollama.Model;

        GitFetchBranch(config.WorkDir, config.SourceBranch);

        var diff = GetDiffUsingGitCli(config.WorkDir, config.SourceBranch, config.TargetBranch);

        var promptFormat =
            @"You are an experienced developer. Conduct a code review. 
Be strict and find all possible problems: bugs, vulnerabilities, style, performance.
Structure your answer with the headings: **Critical issues**, **Suggestions**, **Questions**.


Diff:
{0}";

        var prompt = string.Format(promptFormat, diff);
        Console.WriteLine(prompt);
        Console.WriteLine("====================");

        await foreach (var stream in ollamaApiClient.GenerateAsync(prompt))
            Console.Write(stream?.Response);
    }

    /// <summary>
    /// Retrieves the diff between two branches using the Git command-line interface.
    /// </summary>
    /// <param name="repoPath">The path to the Git repository.</param>
    /// <param name="sourceBranch">The source branch.</param>
    /// <param name="targetBranch">The target branch.</param>
    /// <returns>The diff output as a string.</returns>
    private static string GetDiffUsingGitCli(string repoPath, string sourceBranch, string targetBranch)
    {
        return GitProcessStartInfo(repoPath, $"diff -w {targetBranch}...{sourceBranch}");
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

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed: {error}");
        }

        return output;
    }

    /// <summary>
    /// Binds configuration settings from various sources.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The configured application settings.</returns>
    private static AppSettings BindConfig(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Bind settings to classes
        var appConfig = new AppSettings();
        configuration.Bind(appConfig);
        return appConfig;
    }
}