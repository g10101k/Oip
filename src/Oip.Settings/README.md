# Oip.Settings

Application settings with EFCore provider with priority:

* Command line argument
* Environment variables
* Json file
* EF Core

# Startup

1. Make class with settings

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    public string TestString { get; set; } = "test";
    public int TestInt { get; set; } = 1;
}
````

2. Initialize settings with ConnectionString in in json file, command line arg or other

````csharp
public class Program
{
    public static void Main(string[] args)
    {
        // Initialize settings 
        AppSettings.Initialize(args);

        var builder = WebApplication.CreateBuilder(args);
        // Add settings db context
        builder.Services.AddAppSettingsDbContext(AppSettings.Instance);
        
        var app = builder.Build();
        app.MapGet("/", () => $"AppSettings.Instance.TestInt: {AppSettings.Instance.TestInt}");

        app.Run();
    }
}
````
