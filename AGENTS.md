# Frontend (TypeScript, Angular, PrimeNG)

## CSS, styles and class

Try to use Tailwind for layout.


## oip-common

If oip-common change need execute npm start publish:local

---

# Backend (C# / ASP.NET Controllers)

## Routing Rules (CRITICAL)

For services that generate a TypeScript client, controllers MUST use explicit action-style routes in attributes.

Examples:

- get-{object-name}-by-object
- get-{object-name}-by-id
- create-{object-name}
- update-{object-name}/{id}
- delete-{object-name}/{id}
  where {object-name} name of object (factory, customer, etc.).

## Error Handling (CRITICAL)

All controllers MUST return errors using ApiExceptionResponse.  
Location: Oip.Base/Exceptions/ApiExceptionResponse.cs


# Codex Instructions

## Entity Framework Migrations

When working on EF Core models, DbContext configuration, entity mappings use `dotnet ef migrations add` command for the responsible project/context.

## ASP.NET Web API Client Generation

This repository uses ASP.NET `dotnet run` to generate Angular Web API clients.

When working on API contracts, controllers, DTOs, OpenAPI/Swagger/NSwag configuration, or generated Angular API client files:

- Treat generated Angular API clients as build output.
- Do not manually edit generated Angular client files unless the user explicitly asks.
- Run the backend generation command from the ASP.NET project directory that contains the responsible `.csproj`.
- Use Debug configuration by default.
- Run the generation command outside the sandbox with escalated permissions, because sandboxed execution can prevent the Angular clients from being generated correctly.

Required command:

```bash
dotnet run --configuration Debug
```

When requesting escalation, use a justification like:

```text
Do you want to run the ASP.NET backend outside the sandbox so Angular Web API clients can be generated correctly?
```

Suggested approval prefix:

```json
["dotnet", "run"]
```

After generation:

- Inspect the generated diff.
- Summarize which generated Angular client files changed.
- Do not revert user changes or unrelated working tree changes.
