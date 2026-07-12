# Controller Registration

## Description

OIP allows you to control which ASP.NET Core controllers are exposed by the application.

Registration management is disabled by default. If the application only calls `AddControllersAndView()`, ASP.NET Core
uses its standard controller discovery mechanism and does not filter controllers.

Explicit registration is enabled only when `AddController<TController>()` is used. After the first such call,
the application exposes only controllers that were explicitly added through `AddController<TController>()`.

## When to Use

Use `AddController<TController>()` when a service should expose a limited set of controllers from shared assemblies
or modules. In the current repository, this is required when Oip.Base is used by services such as Oip.Applications
and Oip.Users. Module endpoints such as FolderModule, MenuController, and others must not be available in these
services.

Example:

```csharp
builder.Services
    .AddControllersAndView()
    .AddController<ApplicationsController>()
    .AddController<SecurityController>();
```

In this mode, controllers available in referenced assemblies but not added through `AddController<TController>()`
will not be exposed.

If you do not plan to run the application in distributed mode (`IsStandalone = false`) or filter controller
registration, do not use `AddController<TController>()`, `.AddApplicationControllers()`, or
`.AddBaseServiceControllers()`.
