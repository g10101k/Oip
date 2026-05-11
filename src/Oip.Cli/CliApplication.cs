namespace Oip.Cli;

public static class CliApplication
{
    public static Task<int> Run(
        string[] args,
        string currentDirectory,
        TextReader input,
        TextWriter output,
        TextWriter error)
    {
        try
        {
            var options = CliOptions.Parse(args);
            if (options.ShowHelp)
            {
                output.WriteLine("Usage: oip-module --name Report [--project path/to/App.csproj] [--force]");
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

            var project = ProjectResolver.Resolve(options.ProjectPath, currentDirectory, input, output);
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
}
