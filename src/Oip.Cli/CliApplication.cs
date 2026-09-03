namespace Oip.Cli;

public static class CliApplication
{
    public static Task<int> Run(string[] args, string currentDirectory, TextReader input, TextWriter output,
        TextWriter error)
    {
        try
        {
            var options = CliOptions.Parse(args);
            if (options.ShowHelp)
            {
                WriteHelp(output);
                return Task.FromResult(0);
            }

            var moduleName = options.Name;
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                output.Write("Module name: ");
                moduleName = input.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(moduleName))
            {
                error.WriteLine("Module name is required. Use --name Report or enter it when prompted.");
                return Task.FromResult(1);
            }

            var project = ProjectResolver.Resolve(
                options.ProjectPath,
                currentDirectory,
                input,
                output,
                options.AngularProject);
            var module = ModuleNameNormalizer.Normalize(moduleName);
            var generator = new ModuleGenerator(project, module, options.Force);
            var result = generator.Generate();

            output.WriteLine("OIP module generated.");
            output.WriteLine();
            output.WriteLine("Created files:");
            foreach (var file in result.CreatedFiles)
            {
                output.WriteLine($"  {file}");
            }

            output.WriteLine();
            output.WriteLine("Changed files:");
            foreach (var file in result.ChangedFiles)
            {
                output.WriteLine($"  {file}");
            }

            output.WriteLine();
            output.WriteLine("Next step:");
            output.WriteLine($"  cd {project.ProjectDirectory}");
            output.WriteLine("  dotnet run --configuration Debug");

            return Task.FromResult(0);
        }
        catch (CliException ex)
        {
            error.WriteLine(ex.Message);
            return Task.FromResult(1);
        }
    }

    private static void WriteHelp(TextWriter output)
    {
        output.WriteLine("Command-line tool for generating OIP modules.");
        output.WriteLine();
        output.WriteLine("Usage:");
        output.WriteLine("  oip <name> [options]");
        output.WriteLine("  oip --name Report [options]");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  --name <name>              Module name. Prompted when omitted.");
        output.WriteLine("  --project <path>           Path to the target ASP.NET project file.");
        output.WriteLine("                             Discovered in the current directory when omitted.");
        output.WriteLine("  --angular-project <value>  Angular project name from angular.json or path to");
        output.WriteLine("                             the Angular project directory. Discovered from");
        output.WriteLine("                             SpaRoot, SpaProxyServerUrl and SpaProxyLaunchCommand");
        output.WriteLine("                             when omitted.");
        output.WriteLine("  --force                    Overwrite generated files when they already exist.");
        output.WriteLine("  -h, --help                 Show command usage.");
        output.WriteLine();
        output.WriteLine("Examples:");
        output.WriteLine("  oip Report");
        output.WriteLine("  oip --name Report --project src/Oip.Rtds/Oip.Rtds.csproj --angular-project oip-rtds");
    }
}