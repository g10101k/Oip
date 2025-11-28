## Oip.Base

This library serves as a foundational layer for building modular applications within the OIP ecosystem, providing
extension methods for ASP.NET Core application bootstrapping, module management, and database migration.

**Core Purpose:**

The `Oip.Base` library aims to simplify the development of modular ASP.NET Core applications by providing:

* **Application Bootstrapping:** Extension methods for configuring the `WebApplicationBuilder` and
  `IApplicationBuilder`, including health checks, OpenAPI/Swagger integration, and localization.
* **Module Management:** Mechanisms for discovering, registering, and managing application modules dynamically.
* **Database Migration:**  Support for applying database migrations and ensuring a consistent database schema.
* **Exception Handling:** Centralized exception handling to provide consistent error responses.

### Key Components and Classes:

**1. `OipModuleApplication` Class:**

This static class provides a collection of extension methods for `WebApplicationBuilder` and `IApplicationBuilder`. It
facilitates common tasks like:

* *
  *`CreateModuleBuilder(IBaseOipModuleAppSettings settings)` & `CreateShellBuilder(IBaseOipModuleAppSettings settings)`:
  **  Creates an initialized `WebApplicationBuilder` instance with pre-configured defaults. The `CreateShellBuilder`
  method adds additional configuration related to startup tasks and services for a core application shell.
* **`AddNlog(this WebApplicationBuilder builder)`:** Configures NLog logging for the application, clearing default
  providers.
* **`AddLocalization(this WebApplicationBuilder builder)`:** Configures localization options, supporting English (en)
  and Russian (ru) cultures.
* **`AddOpenApi(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)`:** Adds OpenAPI/Swagger
  support, including documentation generation and configuration. It supports filtering of published APIs based on the
  settings.
* **`AddDefaultHealthChecks(this WebApplicationBuilder builder)`:** Adds default health checks to the application.
* **`MapDefaultEndpoints(this IApplicationBuilder app)`:** Maps health check endpoints for application monitoring.
* **`BuildApp(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)`:**  Builds the application,
  applying configurations and middleware.
* **`AddRequestLocalization(this WebApplication app)`:** Configures request localization based on configured options.
* **`AddExceptionHandler(this WebApplication app)`:** Configures exception handling, returning JSON error responses.
* **`GetRetryPolicy()`:** Provides a Polly retry policy for handling transient HTTP errors, including NotFound and
  InternalServerError.

**2. `WebApplicationBuilderExtension` Class:**

This static class provides extension methods for `IApplicationBuilder`, focused on module and database management.

* **`MigrateDatabase(this IApplicationBuilder app)`:** Applies any pending database migrations using Entity Framework
  Core and registers discovered modules from loaded assemblies.
* **`AddModulesFromAssemblies(OipModuleContext moduleContext)`:** Scans loaded assemblies for module controllers (
  inheriting from `BaseModuleController` or `BaseDbMigrationController`) and registers them in the database.
* **`GetAllLoadedModules()`:** Returns a list of types that are derived from `BaseModuleController` or
  `BaseDbMigrationController`. This is used during module registration.

**3. `IBaseOipModuleAppSettings` Interface:**

This interface represents the application settings used by the library. It encapsulates configuration options for
various components, including:

* `AppSettingsOptions`: Program arguments for the application.
* `OpenApi`: A collection of API settings for OpenAPI/Swagger documentation.
* `ConnectionString`: The database connection string.

**4. `OipModuleContext` Class:**

This class represents the Entity Framework Core database context for the application. It includes entities for managing
modules, such as the `ModuleEntity` class.

### Usage Examples:

**1. Building an Application:**

```csharp
// Retrieve application settings (implementation not shown)
var settings = AppSettings.Initialize();

// Create a module builder
WebApplicationBuilder builder = OipModuleApplication.CreateShellBuilder(settings);

// Build the application
WebApplication app = builder.Build();

// Configure the pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start the application
app.Run();
```

**2. Applying Database Migrations:**

```csharp
using Microsoft.AspNetCore.Builder;

// ... within the Configure method or similar ...

IApplicationBuilder app = builder.Build();

// Apply database migrations
app.MigrateDatabase();

// Configure the pipeline (example)
app.UseHttpsRedirection();
app.UseAuthorization();
app.Run();
```

**3. Adding a Retry Policy to HttpClient:**

```csharp
using Polly;
using Polly.Extensions.Http;

// Get the retry policy
IAsyncPolicy<HttpResponseMessage> retryPolicy = OipModuleApplication.GetRetryPolicy();

// Configure an HttpClient with the retry policy
using (var client = new HttpClient(new HttpPolicyHandler(retryPolicy)))
{
    // Make requests
}
```

### Key Considerations:

* **Configuration:**  The `IBaseOipModuleAppSettings` interface is central to configuring the library. Ensure that the
  settings are properly loaded and configured for your environment.
* **Module Discovery:**  The module discovery mechanism relies on the existence of `BaseModuleController` or
  `BaseDbMigrationController` derived classes. Ensure that these controllers are correctly implemented and included in
  your assemblies.
* **Database Migrations:**  Use Entity Framework Core migrations to manage database schema changes. Ensure that the
  migrations are applied in a controlled manner.
* **Error Handling:**  The centralized exception handling provides a consistent error response format. Customize the
  error handling as needed for your application.

This documentation provides a high-level overview of the `Oip.Base` library. Refer to the source code and related
documentation for more detailed information.