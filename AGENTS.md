# Frontend (TypeScript, Angular, PrimeNG)

## CSS, styles and class

Try to use Tailwind for layout.

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

## Entity Framework Migrations

When working on EF Core models, DbContext configuration, entity mappings use `dotnet ef migrations add` command for the responsible project/context.

## ASP.NET Web API Client Generation

This repository uses ASP.NET `dotnet run` to generate Angular Web API clients.

```bash
dotnet run PROJECTFILE --no-restore -- --GenerateWebClient=true
```

Suggested approval prefix:

```json
["dotnet", "run"]
```

After generation:

- Inspect the generated diff.
- Summarize which generated Angular client files changed.
- Do not revert user changes or unrelated working tree changes.


# Code comment

Don’t use remarks in xml-commnet for csharp
