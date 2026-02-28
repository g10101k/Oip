using System.Diagnostics;
using System.Text;

namespace Oip.CodeReview;

public class GitHelper
{
    /// <summary>
    /// Retrieves the diff between two branches using Git command-line interface.
    /// </summary>
    /// <param name="repoPath">The path to the repository.</param>
    /// <param name="sourceBranch">The source branch name.</param>
    /// <param name="targetBranch">The target branch name.</param>
    /// <param name="excludeFolders">A list of folders to exclude from the diff.</param>
    /// <param name="filePath">Optional file path to limit the diff to a specific file.</param>
    /// <param name="newCodeOnly">Leave only new code</param>
    /// <returns>The diff output as a string.</returns>
    public static string GetDiffUsingGitCli(string repoPath, string sourceBranch, string targetBranch,
        List<string> excludeFolders, string? filePath = null, bool newCodeOnly = false)
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

        foreach (var f in excludeFolders)
            args.Add($":(exclude){f.Trim()}");

        var diffOutput = GitProcessStartInfo(repoPath, args, newCodeOnly);
        
        return newCodeOnly ? FilterNewCodeOnly(diffOutput) : diffOutput;
    }

    /// <summary>
    /// Executes a Git command and returns the output.
    /// </summary>
    /// <param name="repoPath">The path to the Git repository.</param>
    /// <param name="arguments">The arguments to pass to the Git command.</param>
    /// <param name="newCodeOnly">Leave only new code</param>
    /// <returns>The output of the Git command.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Git command fails.</exception>
    private static string GitProcessStartInfo(string repoPath, List<string> arguments, bool newCodeOnly)
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
        bool inNewCodeBlock = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("@@")) // Start of a hunk
            {
                inNewCodeBlock = false;
                continue; // Skip hunk headers
            }
            else if (line.StartsWith("+") && !line.StartsWith("+++")) // New code line
            {
                inNewCodeBlock = true;
                result.AppendLine(line.Substring(1)); // Remove the '+' prefix
            }
            else if (line.StartsWith("-") && !line.StartsWith("---")) // Removed code line
            {
                inNewCodeBlock = false;
                continue; // Skip removed lines
            }
            else if (inNewCodeBlock && !line.StartsWith("\\")) // Context lines within new code block
            {
                result.AppendLine(line);
            }
        }

        return result.ToString().TrimEnd();
    }
}