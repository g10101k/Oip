# Oip.ModuleExample

Example external OIP extension module.

It serves:

- `GET /manifest.json` - extension manifest for registration in the main OIP app.
- `GET /modules/oip-module-example.js` - Web Component bundle.
- `GET /api/status` - sample backend endpoint reached through the OIP extension proxy.

## Run

```bash
dotnet run --project src/Oip.ModuleExample/Oip.ModuleExample.csproj
```

Default development URLs:

- `https://localhost:5010`
- `http://localhost:5011`

## Register in OIP

Start the main OIP app and register the manifest:

```http
POST /api/extension-modules/register-extension-module
Content-Type: application/json

{
  "manifestUrl": "https://localhost:5010/manifest.json"
}
```

Then create a menu item for `OIP Module Example` in OIP admin menu settings.
