using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.AI;
using Oip.Base.Helpers;

namespace Oip.Hil.AiFunctions;

/// <summary>
/// Agent tool that runs shell commands (<c>/bin/sh -c</c>, <c>cmd.exe /c</c> on Windows) in the folder of a workflow
/// step. Commands run as the application process without a sandbox: the folder only sets the working directory.
/// </summary>
public class ShellTools(ILogger<ShellTools> logger)
{
    /// <summary>
    /// Name of the <see cref="RunShellCommandAsync"/> tool.
    /// </summary>
    public static readonly string RunShellCommandToolName =
        nameof(RunShellCommandAsync)[..^"Async".Length].ToSnakeCase();

    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(30);
    private const int MaxOutputLength = 16 * 1024;

    private string _workingDirectory = string.Empty;

    /// <summary>
    /// Returns the shell tool running commands in <paramref name="workingDirectory"/>.
    /// </summary>
    /// <param name="workingDirectory">Folder of the step; files created in it are attached to the step.</param>
    public AITool AsAiTool(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
        return AIFunctionFactory.Create(RunShellCommandAsync, RunShellCommandToolName);
    }

    /// <summary>
    /// Runs the command and returns its exit code, standard output and standard error, both truncated.
    /// </summary>
    /// <param name="command">Shell command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Description("Runs a shell command (/bin/sh -c on Linux and macOS, cmd.exe /c on Windows) in the working " +
                 "directory of the workflow step and returns the exit code, stdout and stderr. Files created in " +
                 "the working directory are attached to the step. Commands time out after 30 seconds.")]
    public async Task<string> RunShellCommandAsync(
        [Description("Command to run, e.g. cat ../input/file.txt or printf '%s' 'text' > summary.md.")]
        string command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command)) return "Command is empty.";

        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
            WorkingDirectory = _workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(OperatingSystem.IsWindows() ? "/c" : "-c");
        startInfo.ArgumentList.Add(command);

        logger.LogInformation("Running shell command in {WorkingDirectory}: {Command}", _workingDirectory, command);
        using var process = Process.Start(startInfo)
                            ?? throw new InvalidOperationException("The shell process was not started");
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(CommandTimeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            if (cancellationToken.IsCancellationRequested) throw;
            return $"Command timed out after {CommandTimeout.TotalSeconds:0} seconds and was killed.";
        }

        var result = new StringBuilder($"exit code {process.ExitCode}");
        Append(result, "stdout", await stdout);
        Append(result, "stderr", await stderr);
        return result.ToString();
    }

    private static void Append(StringBuilder result, string name, string output)
    {
        if (output.Length == 0) return;
        result.Append($"\n{name}:\n");
        result.Append(output.Length > MaxOutputLength
            ? output[..MaxOutputLength] + $"\n... truncated, {output.Length - MaxOutputLength} more characters"
            : output);
    }
}
