# 🧩 Swagger WebClient Generator

## 📖 Description

`SwaggerGenerateWebClientStartupTask` is a **startup task** for ASP.NET Core that automatically checks for changes
in Swagger documentation and, if necessary, **regenerates TypeScript clients** based on the current APIs.

It activates **only in development mode** (`ASPNETCORE_ENVIRONMENT=Development`) and uses
[`swagger-typescript-api`](https://github.com/acacode/swagger-typescript-api) for generation.

---

## ⚙️ How it works

1. When the application starts, the task loads a list of API configurations from
   `appsettings.Development.json`.
2. For each API:
    - Retrieves the current Swagger document.
    - Compares it with the previously saved JSON (by content).
    - If the document has changed — saves the new version and triggers TypeScript client generation.
3. Generation is performed via `npx swagger-typescript-api generate` in an external process.
4. All process logs (`stdout` and `stderr`) are output to `ILogger`.

---

## ⚙️ Example configuration `appsettings.Development.json`

```json
{
  "ApiGenerationSettings": [
    {
      "DocumentName": "base",
      "OutputPath": "./projects/oip-common/src/api"
    },
    {
      "DocumentName": "v1",
      "OutputPath": "./projects/oip/src/api"
    }
  ]
}
```

---

## 📦 Dependency installation

### Backend (C#)

Add the necessary NuGet packages:

```bash
dotnet add package Swashbuckle.AspNetCore.Swagger
dotnet add package Microsoft.OpenApi
```

### Frontend (Node.js)

Navigate to the frontend directory:

```bash
cd ../Oip.WebClient
npm install swagger-typescript-api --save-dev
```

Optionally, install globally:

```bash
npm install -g swagger-typescript-api
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
