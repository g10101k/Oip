# OipCommon

Add assets in angular.json

```json
{
  "glob": "**/*",
  "input": "node_modules/oip-common/assets",
  "output": "/assets"
}
```

Add tailwind config

```js
const primeui = require("tailwindcss-primeui");
module.exports = {
  /* Your config */
  content: [, /* Your config */ "./node_modules/oip-common/**/*.{html,ts,scss,css,js,mjs}"]
  /* Your config */
};
```

Add scss

```sass
@use "../../../node_modules/oip-common/assets/oip-common";
```

Init L10nService to AppComponent

```ts
import { Component, inject, OnInit } from "@angular/core";
import { SecurityService } from "oip-common";
import { RouterOutlet } from "@angular/router";
import { ToastModule } from "primeng/toast";
import { L10nService } from "../../../oip-common/src/services/l10n.service";

@Component({
  selector: "app-root",
  template: `
    <p-toast />
    <router-outlet></router-outlet>
  `,
  standalone: true,
  imports: [ToastModule, RouterOutlet]
})
export class AppComponent implements OnInit {
  private readonly securityService = inject(SecurityService);
  private readonly translateService = inject(L10nService);

  ngOnInit() {
    this.securityService.auth();
    this.translateService.init([
      {
        code: "en",
        name: "English",
        icon: "flag flag-gb"
      },
      {
        code: "ru",
        name: "Русский",
        icon: "flag flag-ru"
      }
    ]);
  }
}
```

Set up routing

`provideOipRoutes` builds the standard route tree: an authenticated shell (`AppLayoutComponent`
guarded by `AuthGuardService` and `moduleAccessGuard`) holding your routes and the built-in pages,
followed by the `unauthorized`, `notfound` and `**` routes. It keeps `**` last, so do not append
anything after the returned array.

```ts
import { oipAuthGuard, provideOipRoutes } from "oip-common";

export const appRoutes = provideOipRoutes({
  children: [
    {
      path: "dashboard/:id",
      loadComponent: () => import("./app/components/dashboard/dashboard.component").then((m) => m.DashboardComponent),
      canActivate: [oipAuthGuard]
    }
  ]
});
```

Use `oipAuthGuard` on every route of your own that requires a signed in user.

All built-in routes are registered by default. Pass `false` to drop one, or a string to move it to
another path — the path must keep the same route parameters:

```ts
provideOipRoutes({
  children: [
    /* ... */
  ],
  features: {
    dbMigration: "rtds-meta-data-context-migration-module/:id",
    modules: false
  }
});
```

| `features` key | Default path                   | Notes                                              |
| -------------- | ------------------------------ | -------------------------------------------------- |
| `access`       | `access`                       | Redirect target of both guards, keep it registered |
| `error`        | `error`                        |                                                    |
| `profile`      | `profile`                      |                                                    |
| `config`       | `config`                       |                                                    |
| `applications` | `applications`                 | Administrators only                                |
| `modules`      | `modules`                      | Administrators only                                |
| `discussion`   | `discussion/:id`               |                                                    |
| `dbMigration`  | `db-migration/:id`             |                                                    |
| `iframeModule` | `iframe-module/:id`            |                                                    |
| `extensions`   | `extensions/:extensionKey/:id` |                                                    |

The remaining options: `layout` replaces the shell component, `rootRoutes` adds routes outside the
shell, `notFoundPath` and `unauthorizedPath` rename those two pages, and `wildcard: false` drops the
`**` route when your application registers its own catch-all.

Every built-in route is also exported on its own — `oipConfigRoute`, `oipModulesRoute` and so on —
for applications that assemble the route tree by hand instead of calling `provideOipRoutes`.
