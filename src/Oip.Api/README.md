# OIP API Modules Library

The library provides basic infrastructure for creating and managing modules in OIP API applications built on ASP.NET
Core.

## Overview

The library contains base controllers and classes for working with application modular architecture, including module
management, security settings, and database migrations.

## Core Components

### 1. BaseModuleController<TSettings>

Base controller for modules providing common functionality:

- **Security Management**: Access rights and roles for modules
- **Module Settings**: JSON serialization/deserialization of settings
- **Base Rights**: Standard access rights (read, admin)

``` csharp
public abstract class BaseModuleController<TSettings>(ModuleRepository moduleRepository)
: ControllerBase where TSettings : class
```

### 2. BaseDbMigrationController<TSettings>

Specialized controller for managing database migrations:

- **Migration Viewing**: Get list of all migrations with their status
- **Migration Application**: Automatic and manual migration application
- **Administrative Access**: Only for users with admin role

``` csharp
public abstract class BaseDbMigrationController<TSettings> : BaseModuleController<TSettings> 
    where TSettings : class
```

### 3. ModuleController

Controller for managing system modules:

- **CRUD Operations**: Add, remove, and get modules
- **Loading Status**: Track loaded modules
- **Administrative Interface**: Full system module management

### 4. FolderModuleController

Example implementation of a specific module - folder module:

- **Inheritance from BaseModuleController**: Using base functionality
- **Specific Settings**: HTML content for the module
- **Custom Rights**: Defining access rights for specific module

## Functionality

### Security Management

- Dynamic management of module access rights
- Role-based security model
- REST API for rights management

### Module Settings

- Flexible settings system via JSON serialization
- Instance-specific settings for each module instance
- Administrative interface for modifying settings

### Database Migrations

- Full control over Entity Framework Core migrations
- View migration status (applied, pending, existing)
- Safe migration application via API

## Usage

### Creating a New Module

``` csharp
[ApiController]
[Route("api/my-module")]
public class MyModuleController : BaseModuleController<MyModuleSettings>
{
    public override List<SecurityResponse> GetModuleRights()
    {
        // Define access rights for the module
    }
}

public class MyModuleSettings
{
    // Specific module settings
}
```

### Working with Migrations

``` csharp
public class MyModuleWithMigrationsController 
    : BaseDbMigrationController<MyModuleSettings>
{
    public MyModuleWithMigrationsController(
        ModuleRepository repository, 
        DbContext dbContext) 
        : base(repository, dbContext)
    {
    }
}
```

## Requirements

- ASP.NET Core
- Entity Framework Core
- Role-based authorization
- JSON serialization (Newtonsoft.Json)

## Security

All administrative operations are protected by the `[Authorize(Roles = SecurityConstants.AdminRole)]` attribute and are
accessible only to users with administrator role.
