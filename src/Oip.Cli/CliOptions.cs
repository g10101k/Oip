namespace Oip.Cli;

public sealed class CliOptions
{
    public string? Name { get; private init; }
    public string? ProjectPath { get; private init; }
    public bool Force { get; private init; }
    public bool ShowHelp { get; private init; }

    public static CliOptions Parse(string[] args)
    {
        string? name = null;
        string? projectPath = null;
        var force = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--name":
                    name = ReadValue(args, ref i, arg);
                    break;
                case "--project":
                    projectPath = ReadValue(args, ref i, arg);
                    break;
                case "--force":
                    force = true;
                    break;
                case "-h":
                case "--help":
                    return new CliOptions { ShowHelp = true };
                default:
                    if (name is null)
                    {
                        name = arg;
                    }
                    else
                    {
                        throw new CliException($"Unknown argument: {arg}");
                    }

                    break;
            }
        }

        return new CliOptions
        {
            Name = name,
            ProjectPath = projectPath,
            Force = force,
            ShowHelp = false
        };
    }

    private static string ReadValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new CliException($"{option} requires a value.");
        }

        index++;
        return args[index];
    }
}
