# Swagger WebClient Generator

## Description

`SwaggerGenerateWebClientStartupTask` generates a TypeScript web API client from Swagger documents when an ASP.NET
Core application starts.

The task runs only when it is registered in DI:

- in services that use `GenerateWebClientStartupTask(settings)`, registration is enabled by the `GenerateWebClient`
  key
- only `OpenApi` items with a non-empty `GenerateCommand` are processed

After generation completes, the task calls `StopApplication()`, so the application does not continue its normal startup.

Important: generated frontend API files are considered read-only. They must be updated through regeneration only, not
by manual edits.

Example generation command:

```shell
dotnet run ./Oip.csproj --no-restore -- --GenerateWebClient=true
```

## How it works

When the application starts with the `GenerateWebClient` argument, `GenerateWebClientStartupTask` follows this flow:

1. Takes all entries from `settings.OpenApi` where `GenerateCommand` is specified.
2. For each entry, gets the Swagger JSON.
3. Saves the Swagger JSON to a file.
4. Runs the external generation command, passing the file path through `{SwaggerJsonPath}`.
5. Deletes the Swagger JSON file.

If an error happens for a specific configuration, it is logged and then rethrown.
After completion, the task always logs `Swagger generation complete`, then the application is stopped through
`StopApplication()`.

## Where Swagger Is Stored

The Swagger JSON is saved only as a temporary input file for the generation command.

The file is created at:

```text
obj/SwaggerFiles/swagger-{name}.json
```

Example:

```text
obj/SwaggerFiles/swagger-base.json
obj/SwaggerFiles/swagger-v1.json
```

After the command completes, the file is deleted.

## `OpenApi` configuration

Key `OpenApiItem` fields:

- `Name` - Swagger group name used in `GetSwagger(config.Name)`
- `Publish` - whether the Swagger document is published
- `Url` - Swagger JSON URL
- `Version` - API version
- `Title` - API title
- `Description` - API description
- `GenerateCommand` - web API client generation command
- `ForceGeneration` - obsolete field

To include a controller in the correct Swagger group, use the attribute:

```csharp
[ApiExplorerSettings(GroupName = "base")]
```

The `GroupName` value must match `OpenApi[].Name`.

## Example `appsettings.Development.json`

The current generation configuration format looks like this:

```json
{
  "OpenApi": [
    {
      "Publish": true,
      "Name": "v1",
      "Url": "/swagger/v1/swagger.json",
      "Version": "v1.0.0",
      "Title": "Oip service web-api",
      "Description": "Oip service web-api",
      "GenerateCommand": "node ./node_modules/oip-common/scripts/generate-api.mjs -c -i {SwaggerJsonPath} -t ./node_modules/oip-common/templates -o ./projects/oip/src/api"
    },
    {
      "Publish": true,
      "Name": "base",
      "Url": "/swagger/base/swagger.json",
      "Version": "v1.0.0",
      "Title": "Base service web-api",
      "Description": "Base service web-api",
      "GenerateCommand": "node ./node_modules/oip-common/scripts/generate-api.mjs -i {SwaggerJsonPath} -t ./node_modules/oip-common/templates -o ./projects/oip-common/src/api"
    }
  ]
}
```

## How `GenerateCommand` is executed

`GenerateCommand` is run as an external process.

Execution details:

- before execution, the `{SwaggerJsonPath}` placeholder is replaced with the path to the temporary Swagger JSON
- if the path contains spaces, it is automatically wrapped in quotes
- the process working directory is taken from `settings.SpaProxyServer.WorkingDirectory`
- `NODE_TLS_REJECT_UNAUTHORIZED=0` is added to the process environment
- `stdout` is written to `ILogger` as `Information`
- `stderr` is written to `ILogger` as `Error`

On Windows, the command is additionally started through:

```text
cmd.exe /c
```

Because of that, the command must be written so it can be executed from `SpaProxyServer.WorkingDirectory`.

## `generate-api.mjs` logic

In the current project, `GenerateCommand` usually calls this script:

```text
./projects/oip-common/scripts/generate-api.mjs
```

This script is a wrapper around `swagger-typescript-api` and defines common generation rules for the frontend.

### Supported arguments

The script accepts these main parameters:

- `-i`, `--input` - path to the local Swagger JSON file
- `-o`, `--output` - output folder for generated files
- `-t`, `--templates` - path to custom templates
- `-d`, `--data-contract-prefix` - prefix for the `data-contracts` file, allowing multiple different APIs to be
  generated into the same directory
- `-c`, `--use-common-client` - use a shared HTTP client instead of generating a dedicated one

### Base generation settings

The script calls `generateApi()` with these important options:

- `httpClientType: "fetch"`
- `generateResponses: true`
- `extractRequestParams: true`
- `extractRequestBody: true`
- `extractEnums: true`
- `unwrapResponseData: true`
- `modular: true`
- `moduleNameFirstTag: true`
- `cleanOutput: false`
- `defaultResponseType: "void"`

This means the generation:

- is based on `fetch`
- extracts request params, request body, enums, and response types into separate structures
- generates a modular client
- does not clean the output directory before running
- uses the first Swagger tag as the module name

### Type customization

The script also defines additional type conversion rules:

- OpenAPI `string` with the `date-time` format is converted to the TypeScript `Date` type
- invalid generated names are fixed with the `Type` and `Value` prefixes

It also defines a custom `RecordType` that returns:

```ts
MyRecord<key, value>
```

If project templates and types rely on `MyRecord`, this behavior must be considered when changing the generator.

### File naming rules

After generation, the script additionally renames output files:

- `data-contracts.ts` is saved as `{dataContractPrefix}data-contracts.ts`
- `http-client.ts` keeps its original name
- all other files are converted to `kebab-case` and get the `.api` suffix

Examples:

- `UserProfile` -> `user-profile.api.ts`
- `Security` -> `security.api.ts`

### `use common client` mode

If the `-c` / `--use-common-client` flag is passed:

- `useCommonClient: true` is set in the generator configuration
- `http-client.ts` is not written to disk

In this mode, the frontend uses a shared HTTP client from the project infrastructure.

### File writing

The script:

- resolves `input`, `templates`, and `output` relative to `process.cwd()`
- creates the output directory if needed
- writes each generated file separately
- prints service messages like `File create: ...` to the console

Because `cleanOutput: false`, old files that are no longer generated are not removed automatically. This is important
when changing the API structure or templates.

## What is outdated

Documentation and configuration must not rely on `WebClientOutputPath`, `ForceGeneration`, or a persistent Swagger
snapshot.

In the current implementation, generation is controlled only through `GenerateCommand`. This command defines:

- which generator is used
- which templates are applied
- which directory receives the output
- which additional flags are enabled

In other words, generation logic is now defined by the command configuration, not by a separate output path field.
