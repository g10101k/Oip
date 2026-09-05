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
| `noModules`    | `no-modules`                   | Shown when the user has no module available        |
| `start`        | `` (the shell root)            | Redirects to the start module, see below           |

The remaining options: `layout` replaces the shell component, `rootRoutes` adds routes outside the
shell, `notFoundPath` and `unauthorizedPath` rename those two pages, and `wildcard: false` drops the
`**` route when your application registers its own catch-all.

Open a module by default

The empty path is claimed by a redirect route that resolves the module instance the user lands on.
It picks, in order: the `startRoute` of the host application, the module the user marked as their
start page, the first module of their menu, and finally the `no-modules` page. The menu returned by
the backend is already filtered by rights, so the resolved module is always one the user may open —
a module that was deleted or whose rights were revoked simply drops out and the next candidate wins.

```ts
provideOipRoutes({
  children: [
    /* ... */
  ],
  startRoute: "/dashboard/1"
});
```

Leave `startRoute` unset to follow the user's own choice. Users pick it from the sidebar: right
click a menu item and choose *Set as start page*, which stores it per user on the backend and marks
the item with a star. The route is skipped when `children` already declares its own empty path, and
`features: { start: false }` drops it entirely.

`StartPageService` backs all of this — `resolveStartUrl()` returns the target as a `UrlTree` (or
`null` when nothing is available), `setStartModule(id)` and `clearStartModule()` change the choice.
Results are cached per user and dropped when the session or the choice changes.

Every built-in route is also exported on its own — `oipConfigRoute`, `oipModulesRoute` and so on —
for applications that assemble the route tree by hand instead of calling `provideOipRoutes`.

Block the UI during module transitions

`AppLayoutComponent` renders `BlockLoaderComponent`, a full screen blocker driven by
`ModuleLoadingService`. It covers router navigation on its own, plus the module bootstrap that
`BaseModuleComponent` runs after `NavigationEnd` — rights, settings, `onModuleInstanceChange()` and
extension loading — so nothing is clickable while a module is still coming up. Applications using
the standard shell get it for free; only a custom `layout` component has to render
`<block-loader />`, and it must sit outside `.layout-wrapper`, which the blocker marks `inert`
while it is visible.

Register your own long running work with `ModuleLoadingService` when it must block the same way:

```ts
import { Component, inject } from "@angular/core";
import { BaseModuleComponent, ModuleLoadingService } from "oip-common";

@Component({
  /* ... */
})
export class ReportModuleComponent extends BaseModuleComponent<ReportSettings, void> {
  private readonly moduleLoadingService = inject(ModuleLoadingService);

  protected async runReport(): Promise<void> {
    await this.moduleLoadingService.track(this.reportApi.build(this.id));
  }
}
```

`track()` releases the blocker when the promise rejects, so an error never leaves the UI locked. The
`begin()` / `end()` pair is available for work that is not a promise; balance it from a `finally`
block. A `begin()` that is never released is dropped after 30 seconds with a console warning.

The blocker appears only when a transition lasts longer than 150 ms and then stays for at least
300 ms, so fast navigation does not flash a spinner.
