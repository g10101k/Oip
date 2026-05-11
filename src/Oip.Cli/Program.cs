using Oip.Cli;

var result = await CliApplication.Run(args, Directory.GetCurrentDirectory(), Console.In, Console.Out, Console.Error);
return result;
