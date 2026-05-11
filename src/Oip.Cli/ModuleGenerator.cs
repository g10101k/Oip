namespace Oip.Cli;

public sealed class ModuleGenerator(TargetProject project, ModuleName module, bool force)
{
    public ModuleGenerationResult Generate()
    {
        var controllerPath = Path.Combine(project.ProjectDirectory, "Controllers", $"{module.ControllerClassName}.cs");
        var componentDirectory = Path.Combine(
            project.AngularProjectPath,
            "src",
            "app",
            "components",
            module.ComponentFolder);
        var componentPath = Path.Combine(componentDirectory, module.ComponentFileName);
        var routesPath = Path.Combine(project.AngularProjectPath, "src", "app.routes.ts");
        var i18nDirectory = Path.Combine(project.AngularProjectPath, "src", "assets", "i18n");
        var englishI18nPath = Path.Combine(i18nDirectory, $"{module.RoutePath}.en.json");
        var russianI18nPath = Path.Combine(i18nDirectory, $"{module.RoutePath}.ru.json");

        EnsureNoConflicts(controllerPath, componentPath, routesPath, englishI18nPath, russianI18nPath);

        var routeContent = File.ReadAllText(routesPath);
        var routeResult = AngularRouteEditor.InsertModuleRoute(routeContent, module);
        if (routeResult.Status == RouteInsertionStatus.Duplicate)
        {
            throw new CliException($"Angular route already exists: {module.RoutePath}/:id");
        }

        if (routeResult.Status == RouteInsertionStatus.ChildrenNotFound || routeResult.Content is null)
        {
            throw new CliException($"Could not find a children route array in {routesPath}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(controllerPath)!);
        Directory.CreateDirectory(componentDirectory);
        Directory.CreateDirectory(i18nDirectory);

        File.WriteAllText(controllerPath, BuildController());
        File.WriteAllText(componentPath, BuildComponent());
        File.WriteAllText(englishI18nPath, BuildEnglishI18n());
        File.WriteAllText(russianI18nPath, BuildRussianI18n());
        File.WriteAllText(routesPath, routeResult.Content);

        return new ModuleGenerationResult(
            [controllerPath, componentPath, englishI18nPath, russianI18nPath],
            [routesPath]);
    }

    private void EnsureNoConflicts(
        string controllerPath,
        string componentPath,
        string routesPath,
        string englishI18nPath,
        string russianI18nPath)
    {
        if (!File.Exists(routesPath))
        {
            throw new CliException($"Angular routes file was not found: {routesPath}");
        }

        var routeContent = File.ReadAllText(routesPath);
        if (routeContent.Contains($"path: '{module.RoutePath}/:id'", StringComparison.Ordinal))
        {
            throw new CliException($"Angular route already exists: {module.RoutePath}/:id");
        }

        if (File.Exists(controllerPath) && !force)
        {
            throw new CliException($"Controller already exists: {controllerPath}. Use --force to overwrite it.");
        }

        if (File.Exists(componentPath) && !force)
        {
            throw new CliException($"Component already exists: {componentPath}. Use --force to overwrite it.");
        }

        if (File.Exists(englishI18nPath) && !force)
        {
            throw new CliException($"English translations already exist: {englishI18nPath}. Use --force to overwrite them.");
        }

        if (File.Exists(russianI18nPath) && !force)
        {
            throw new CliException($"Russian translations already exist: {russianI18nPath}. Use --force to overwrite them.");
        }

        var controllersDirectory = Path.Combine(project.ProjectDirectory, "Controllers");
        if (!Directory.Exists(controllersDirectory))
        {
            return;
        }

        var controllerRoute = $"[Route(\"{module.ControllerRoute}\")]";
        foreach (var controllerFile in Directory.GetFiles(controllersDirectory, "*.cs"))
        {
            if (File.ReadAllText(controllerFile).Contains(controllerRoute, StringComparison.Ordinal) &&
                !Path.GetFullPath(controllerFile).Equals(Path.GetFullPath(controllerPath), StringComparison.OrdinalIgnoreCase))
            {
                throw new CliException($"Controller route already exists in {controllerFile}: {module.ControllerRoute}");
            }
        }
    }

    private string BuildController()
    {
        return $$"""
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Api.Controllers.Api;
using Oip.Data.Constants;
using Oip.Data.Repositories;

namespace {{project.RootNamespace}}.Controllers;

/// <summary>
/// Controller for the {{module.BaseName}} module.
/// </summary>
[ApiController]
[Route("{{module.ControllerRoute}}")]
public class {{module.ControllerClassName}}(ModuleRepository moduleRepository)
    : BaseModuleController<{{module.SettingsClassName}}>(moduleRepository)
{
    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = "Read",
                Description = "Can view this module",
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}

/// <summary>
/// Settings for the {{module.BaseName}} module.
/// </summary>
public class {{module.SettingsClassName}}
{
}
""";
    }

    private string BuildComponent()
    {
        return $$$"""
import { Component } from '@angular/core';
import { BaseModuleComponent, NoSettingsDto, SecurityComponent } from 'oip-common';
import { {{{module.SettingsClassName}}} } from '../../../api/data-contracts';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-{{{module.RoutePath}}}',
  imports: [SecurityComponent, TranslatePipe, Button, InputText, ReactiveFormsModule, FormsModule],
  template: `
    @if (isContent) {
      <div class="card space-y-4">
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h5 class="mb-1">{{ title }}</h5>
            <p class="m-0 text-surface-500">{{ '{{{module.RoutePath}}}.content.subtitle' | translate }}</p>
          </div>
        </div>
      </div>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ '{{{module.RoutePath}}}.settings.title' | translate }}</div>
            <div class="grid grid-cols-12 gap-4">
              <label class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0" for="dayCount">
                {{ '{{{module.RoutePath}}}.settings.dayCount' | translate }}
              </label>
              <div class="col-span-12 md:col-span-10">
                <input id="dayCount" pInputText type="text" [(ngModel)]="settings"/>
              </div>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                [label]="'{{{module.RoutePath}}}.settings.save' | translate"
                (onClick)="saveSettings(settings)"></p-button>
            </div>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `
})
export class {{{module.ComponentClassName}}} extends BaseModuleComponent<{{{module.SettingsClassName}}}, NoSettingsDto> {
  protected override async onModuleInstanceChange(): Promise<void> {
    // Initialize module data here.
  }
}
""";
    }

    private string BuildEnglishI18n()
    {
        return $$"""
{
  "{{module.RoutePath}}": {
    "title": "{{module.BaseName}} Module",
    "content": {
      "subtitle": "{{module.BaseName}} module content."
    },
    "settings": {
      "title": "{{module.BaseName}} Module Settings",
      "dayCount": "Day Count",
      "save": "Save"
    }
  }
}
""";
    }

    private string BuildRussianI18n()
    {
        return $$"""
{
  "{{module.RoutePath}}": {
    "title": "Модуль {{module.BaseName}}",
    "content": {
      "subtitle": "Содержимое модуля {{module.BaseName}}."
    },
    "settings": {
      "title": "Настройки модуля {{module.BaseName}}",
      "dayCount": "Количество дней",
      "save": "Сохранить"
    }
  }
}
""";
    }
}
