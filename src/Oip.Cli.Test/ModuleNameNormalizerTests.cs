namespace Oip.Cli.Test;

public class ModuleNameNormalizerTests
{
    [TestCase("Report", "ReportModuleController", "ReportModuleSettings", "ReportModuleComponent", "report")]
    [TestCase("report", "ReportModuleController", "ReportModuleSettings", "ReportModuleComponent", "report")]
    [TestCase("Reports", "ReportModuleController", "ReportModuleSettings", "ReportModuleComponent", "report")]
    [TestCase("Report Module", "ReportModuleController", "ReportModuleSettings", "ReportModuleComponent", "report")]
    [TestCase("WeatherForecastModule", "WeatherForecastModuleController", "WeatherForecastModuleSettings", "WeatherForecastModuleComponent", "weather-forecast")]
    public void Normalize_BuildsExpectedNames(
        string input,
        string controller,
        string settings,
        string component,
        string kebab)
    {
        var result = ModuleNameNormalizer.Normalize(input);

        Assert.That(result.ControllerClassName, Is.EqualTo(controller));
        Assert.That(result.SettingsClassName, Is.EqualTo(settings));
        Assert.That(result.ComponentClassName, Is.EqualTo(component));
        Assert.That(result.KebabName, Is.EqualTo(kebab));
        Assert.That(result.RoutePath, Is.EqualTo($"{kebab}-module"));
    }
}
