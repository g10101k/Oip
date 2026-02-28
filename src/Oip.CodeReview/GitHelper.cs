using System.Diagnostics;
using System.Text;

namespace Oip.CodeReview;

/// <summary>
/// Provides helper methods for interacting with Git repositories.
/// </summary>
public class GitHelper
{
    /// <summary>
    /// Retrieves the diff between two branches using Git command-line interface.
    /// </summary>
    /// <param name="repoPath">The path to the repository.</param>
    /// <param name="sourceBranch">The source branch name.</param>
    /// <param name="targetBranch">The target branch name.</param>
    /// <param name="excludePatterns">A list of folders to exclude from the diff.</param>
    /// <param name="filePath">Optional file path to limit the diff to a specific file.</param>
    /// <param name="newCodeOnly">Leave only new code</param>
    /// <returns>The diff output as a string.</returns>
    public static string GetDiffUsingGitCli(string repoPath, string sourceBranch, string targetBranch,
        List<string> excludePatterns, string? filePath = null, bool newCodeOnly = false)
    {
        List<string> args =
        [
            "diff",
            "-w",
            "--inter-hunk-context=100",
            "--ignore-all-space",
            $"{targetBranch}...{sourceBranch}",
        ];

        if (!string.IsNullOrEmpty(filePath))
            args.Add(filePath);

        foreach (var f in excludePatterns)
            args.Add($":(exclude){f.Trim()}");

        var diffOutput = GitProcessStartInfo(repoPath, args);

        return newCodeOnly ? FilterNewCodeOnly(diffOutput) : diffOutput;
    }

    /// <summary>
    /// Executes a Git command and returns the output.
    /// </summary>
    /// <param name="repoPath">The path to the Git repository.</param>
    /// <param name="arguments">The arguments to pass to the Git command.</param>
    /// <returns>The output of the Git command.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Git command fails.</exception>
    private static string GitProcessStartInfo(string repoPath, List<string> arguments)
    {
        var absolutePath = string.IsNullOrEmpty(repoPath)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(repoPath);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = absolutePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in arguments)
            processStartInfo.ArgumentList.Add(arg);

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return process.ExitCode != 0 ? throw new InvalidOperationException($"Command failed: {error}") : output;
    }


    /// <summary>
    /// Filters diff output to show only new code (added lines)
    /// </summary>
    /// <param name="diffOutput">The full diff output</param>
    /// <returns>Only the new code from the diff</returns>
    private static string FilterNewCodeOnly(string diffOutput)
    {
        if (string.IsNullOrEmpty(diffOutput))
            return diffOutput;

        var lines = diffOutput.Split('\n');
        var result = new StringBuilder();

        foreach (var line in lines)
        {
            if (line.StartsWith("+") && !line.StartsWith("+++"))
            {
                result.AppendLine(line.Remove(0, 1).Insert(0, " "));
                continue;
            }
            else if (line.StartsWith("-") && !line.StartsWith("---"))
            {
                continue;
            }

            result.AppendLine(line);
        }

        return result.ToString().TrimEnd();
    }
}