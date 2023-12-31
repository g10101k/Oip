using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Oip.Settings.Migrations;

/// <summary>
/// Settings model snapshot
/// </summary>
public abstract class SettingsModelSnapshot : ModelSnapshot
{
    /// <summary>
    /// Database schema
    /// </summary>
    protected string AppSettingsSchema =>
        Environment.GetEnvironmentVariable(nameof(AppSettingsOptions.AppSettingsSchema)) ?? "settings";

    /// <summary>
    /// Application settings table
    /// </summary>
    protected string AppSettingsTable => 
        Environment.GetEnvironmentVariable(nameof(AppSettingsOptions.AppSettingsTable)) ?? "AppSetting";
}