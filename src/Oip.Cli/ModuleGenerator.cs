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

        EnsureNoConflicts(controllerPath, componentPath, routesPath);

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

        File.WriteAllText(controllerPath, BuildController());
        File.WriteAllText(componentPath, BuildComponent());
        File.WriteAllText(routesPath, routeResult.Content);

        return new ModuleGenerationResult(
            [controllerPath, componentPath],
            [routesPath]);
    }

    private void EnsureNoConflicts(string controllerPath, string componentPath, string routesPath)
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
import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { BaseModuleComponent, NoSettingsDto, SecurityComponent } from 'oip-common';

@Component({
  selector: 'app-{{{module.RoutePath}}}',
  imports: [NgIf, SecurityComponent],
  template: `
    <div *ngIf="isContent" class="flex min-h-64 flex-col gap-4 rounded border border-surface-200 bg-surface-0 p-6">
      <h5 class="m-0">{{ title }}</h5>
      <div class="flex flex-1 items-center justify-center text-surface-500">
        {{{module.BaseName}}} module content
      </div>
    </div>

    <security *ngIf="isSecurity" [controller]="controller" [id]="id" />
  `
})
export class {{{module.ComponentClassName}}} extends BaseModuleComponent<NoSettingsDto, NoSettingsDto> {}
""";
    }
}
