namespace Oip.Cli.Test;

public class ModuleGeneratorTests
{
    private string _temporaryDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), "oip-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, true);
        }
    }

    [Test]
    public void Generate_UsesBackendSettingsDtoInAngularComponent()
    {
        var project = CreateProject();
        var module = ModuleNameNormalizer.Normalize("Invoice");

        new ModuleGenerator(project, module, false).Generate();

        var componentPath = Path.Combine(
            project.AngularProjectPath,
            "src",
            "app",
            "components",
            "invoice-module",
            "invoice-module.component.ts");
        var component = File.ReadAllText(componentPath);

        Assert.That(component, Does.Contain("import { InvoiceModuleSettings } from '../../../api/data-contracts';"));
        Assert.That(component, Does.Contain("import { TranslatePipe } from '@ngx-translate/core';"));
        Assert.That(component, Does.Contain("imports: [SecurityComponent, TranslatePipe, Button, InputText, ReactiveFormsModule, FormsModule]"));
        Assert.That(component, Does.Contain("{{ 'invoice-module.content.subtitle' | translate }}"));
        Assert.That(component, Does.Contain("@else if (isSettings)"));
        Assert.That(component, Does.Contain("{{ 'invoice-module.settings.dayCount' | translate }}"));
        Assert.That(component, Does.Contain("[label]=\"'invoice-module.settings.save' | translate\""));
        Assert.That(
            component,
            Does.Contain("extends BaseModuleComponent<InvoiceModuleSettings, NoSettingsDto>"));
        Assert.That(component, Does.Contain("protected override async onModuleInstanceChange(): Promise<void>"));
        Assert.That(component, Does.Contain("// Initialize module data here."));

        var englishI18nPath = Path.Combine(project.AngularProjectPath, "src", "assets", "i18n", "invoice-module.en.json");
        var russianI18nPath = Path.Combine(project.AngularProjectPath, "src", "assets", "i18n", "invoice-module.ru.json");
        var englishI18n = File.ReadAllText(englishI18nPath);
        var russianI18n = File.ReadAllText(russianI18nPath);

        Assert.That(englishI18n, Does.Contain("\"invoice-module\""));
        Assert.That(englishI18n, Does.Contain("\"subtitle\": \"Invoice module content.\""));
        Assert.That(englishI18n, Does.Contain("\"dayCount\": \"Day Count\""));
        Assert.That(englishI18n, Does.Contain("\"save\": \"Save\""));
        Assert.That(russianI18n, Does.Contain("\"invoice-module\""));
        Assert.That(russianI18n, Does.Contain("\"dayCount\": \"Количество дней\""));
        Assert.That(russianI18n, Does.Contain("\"save\": \"Сохранить\""));
    }

    private TargetProject CreateProject()
    {
        var projectDirectory = Path.Combine(_temporaryDirectory, "Oip");
        var angularProjectPath = Path.Combine(_temporaryDirectory, "Oip.WebClient", "projects", "oip");
        Directory.CreateDirectory(Path.Combine(angularProjectPath, "src"));
        File.WriteAllText(
            Path.Combine(angularProjectPath, "src", "app.routes.ts"),
            """
            import { Routes } from '@angular/router';

            export const appRoutes: Routes = [
              {
                path: '',
                children: []
              }
            ];
            """);

        return new TargetProject(
            Path.Combine(projectDirectory, "Oip.csproj"),
            projectDirectory,
            "Oip",
            "Oip",
            Path.Combine(_temporaryDirectory, "Oip.WebClient"),
            angularProjectPath);
    }
}
