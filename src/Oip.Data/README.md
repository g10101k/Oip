# OIP Data Library

A library for managing modules in ASP.NET Core applications, providing capabilities for automatic module discovery,
registration, and management through a database.

## Key Features

### Module Discovery

- Automatic scanning of loaded assemblies to find modules
- Support for controllers inheriting from `BaseModuleController` and `BaseDbMigrationController`
- Automatic updating of module metadata in the database

### Module Management

- CRUD operations for modules and their instances
- Module settings management
- Hierarchical module structure in menus

### Security

- Role-based access model for modules
- Access right management ("read" and others)
- Module filtering based on user roles

## Installation and Configuration

### Database Migration

To apply migrations and register modules, add to the `Program.cs` method:

```csharp
var app = builder.Build();

// Applying migrations and registering modules
app.MigrateDatabase();
```

### Repository Usage

Register the repository in the DI container:

```csharp
builder.Services.AddScoped<ModuleRepository>();
```

## Module Configuration

### Automatic Registration

Modules are automatically discovered based on the following criteria:

- Inherit from `BaseModuleController` or `BaseDbMigrationController`
- Have a `[Route]` attribute for path definition
- Module name is derived from the controller name (without the "Controller" suffix)

### Manual Management

Use `ModuleRepository` for manual management:

- Adding/removing modules
- Security configuration
- Settings updates

## Usage Example

```csharp
public class MyModuleController : BaseModuleController
{
    [Route("api/mymodule")]
    public IActionResult Get()
    {
        return Ok("My module is working!");
    }
}
```

This module will be automatically discovered and registered with the name "MyModule" and path "/mymodule".

## Security

The library supports a role-based security model:

- Each module can have security settings
- Access rights are defined by user roles
- Available modules are filtered based on roles

Use `ModuleRepository` methods to manage access rights for specific modules.