using Oip.Settings;
using Oip.Settings.Example;

var settings = AppSettings.Initialize(new AppSettingsOptions()
{
    ProgrammeArguments = args,
    AppSettingsTable = "TestSettingsTableName",
}); 

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAppSettingsDbContext(settings);

var app = builder.Build();
app.MapGet("/", () => $"AppSettings.Instance.TestInt: {settings.TestInt}");

app.Run();