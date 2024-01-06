using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Settings.Migrations;

/// <summary>
/// Settings migration
/// </summary>
public abstract class SettingsMigration : Migration
{
    // I tried implementing dynamic migrations on EFCore, but for some reason the singleton AppSettings.Instance
    // was empty when calling Migrate(). This is a fucking point, but I couldn’t decide otherwise.
    // If you know how to do it more cleanly, don’t be lazy and help.
    
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