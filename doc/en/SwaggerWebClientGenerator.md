# 🧩 Swagger WebClient Generator

## 📖 Description

`SwaggerGenerateWebClientStartupTask` is a **startup task** for ASP.NET Core that automatically checks for changes in
Swagger documentation and, if necessary, **regenerates TypeScript clients** based on the current APIs.

It activates **only in development mode** (`ASPNETCORE_ENVIRONMENT=Development`) and uses
[`swagger-typescript-api`](https://github.com/acacode/swagger-typescript-api) for generation.

To correctly associate a controller with a specific API, you need to use
the attribute `[ApiExplorerSettings(GroupName = "users")]`.

---

## ⚙️ How it works

1. When the application starts, the task loads the list of API configurations from
   `appsettings.Development.json`.
2. For each API:
    - Retrieves the current Swagger document.
    - Compares it with the previously saved JSON (by content).
    - If the document has changed — saves the new version and triggers TypeScript client generation.
3. Generation is performed via `npx swagger-typescript-api generate` in an external process.
4. All process logs (`stdout` and `stderr`) are output to `ILogger`.

### Important for controller routes

For services that generate a TypeScript client, keep explicit `action-style` routes in controller attributes, for
example: `get-by-object`, `get-by-id`, `create`, `update/{id}`, `delete/{id}`.

Switching to "pure" REST-style paths without these suffixes can degrade or break client generation and the expected
shape of generated methods.

---

## ⚙️ Example configuration `appsettings.Development.json`

```json
{
  "OpenApi": [
    {
      "Publish": true,
      "Name": "base",
      "Url": "/swagger/base/swagger.json",
      "Version": "v1.0.0",
      "Title": "Base service web-api",
      "Description": "Base service web-api",
      "WebClientOutputPath": "./projects/oip/src/api" // <- generates web client if present
    },
    {
      "Publish": true,
      "Name": "users",
      "Url": "/swagger/users/swagger.json",
      "Version": "v1.0.0",
      "Title": "User service web-api",
      "Description": "User service web-api",
      "WebClientOutputPath": "./projects/oip-common/src/user-api" // <- generates web client if present
    }
  ]
}
```

---

## 🧠 Key generation arguments

During generation, the task runs the following command:

```bash
npx swagger-typescript-api generate   -p <swagger.json>   -o <outputPath>   --unwrap-response-data   --extract-enums   --extract-responses   --extract-request-body   --extract-request-params   --modular   --module-name-first-tag   --t ./templates
```

🔹 `--t ./templates` — path to custom templates (if needed).  
🔹 `--unwrap-response-data` — extracts useful payload from API responses.  
🔹 `--extract-*` flags — export enums, request, and response types into separate files.

---

## 🧩 Templates folder (`templates`)

You can override the structure or style of the generated files by adding your own templates in `Oip.WebClient/templates`.  
Template format is the same as used by `swagger-typescript-api` (Handlebars, `.hbs`).

---
