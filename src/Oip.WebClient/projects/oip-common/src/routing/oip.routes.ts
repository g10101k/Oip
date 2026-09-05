 import { inject, Type } from '@angular/core';
import { CanActivateFn, Route, Routes } from '@angular/router';
import { AppLayoutComponent } from '../components/app.layout/app.layout.component';
import { AccessComponent } from '../components/auth/access/access.component';
import { NotfoundComponent } from '../components/notfound/notfound.component';
import { AuthGuardService } from '../services/auth-guard.service';
import { moduleAccessGuard } from '../services/module-access-guard.service';

/**
 * Guard that requires an authenticated session and preserves the requested url as the return url.
 *
 * Use it on every application route rendered inside the oip shell instead of repeating the
 * `inject(AuthGuardService)` lambda.
 */
export const oipAuthGuard: CanActivateFn = (_, state) => inject(AuthGuardService).canActivate(state.url);

/**
 * Access denied page. Referenced by {@link AuthGuardService} and {@link moduleAccessGuard} redirects,
 * so keep it registered unless the host application provides its own `access` route.
 */
export function oipAccessRoute(path = 'access'): Route {
  return { path, component: AccessComponent };
}

/** Authentication error page. */
export function oipErrorRoute(path = 'error'): Route {
  return {
    path,
    loadComponent: () => import('../components/auth/error/error.component').then((m) => m.ErrorComponent)
  };
}

/** Current user profile. */
export function oipProfileRoute(path = 'profile'): Route {
  return {
    path,
    loadComponent: () =>
      import('../components/user-profile/user-profile.component').then((m) => m.UserProfileComponent),
    canActivate: [oipAuthGuard]
  };
}

/** Application configuration. */
export function oipConfigRoute(path = 'config'): Route {
  return {
    path,
    loadComponent: () => import('../components/config/config.component').then((m) => m.ConfigComponent),
    canActivate: [oipAuthGuard]
  };
}

/** Registered applications, administrators only. */
export function oipApplicationsRoute(path = 'applications'): Route {
  return {
    path,
    loadComponent: () =>
      import('../components/applications/applications.component').then((m) => m.ApplicationsComponent),
    canActivate: [oipAuthGuard],
    data: { requireAdmin: true }
  };
}

/** Module registry, administrators only. */
export function oipModulesRoute(path = 'modules'): Route {
  return {
    path,
    loadComponent: () => import('../components/app-modules/app-modules.component').then((m) => m.AppModulesComponent),
    canActivate: [oipAuthGuard],
    data: { requireAdmin: true }
  };
}

/** Discussion module. The path must keep an `:id` segment. */
export function oipDiscussionRoute(path = 'discussion/:id'): Route {
  return {
    path,
    loadComponent: () => import('../components/discussion/discussion.component').then((m) => m.DiscussionComponent),
    canActivate: [oipAuthGuard]
  };
}

/** Database migration module. The path must keep an `:id` segment. */
export function oipDbMigrationRoute(path = 'db-migration/:id'): Route {
  return {
    path,
    loadComponent: () =>
      import('../components/db-migration/db-migration.component').then((m) => m.DbMigrationComponent),
    canActivate: [oipAuthGuard]
  };
}

/** Iframe module host. The path must keep an `:id` segment. */
export function oipIframeModuleRoute(path = 'iframe-module/:id'): Route {
  return {
    path,
    loadComponent: () =>
      import('../components/iframe-module/iframe-module.component').then((m) => m.IframeModuleComponent),
    canActivate: [oipAuthGuard]
  };
}

/** Extension module host. The path must keep the `:extensionKey` and `:id` segments. */
export function oipExtensionsRoute(path = 'extensions/:extensionKey/:id'): Route {
  return {
    path,
    loadComponent: () =>
      import('../components/extension-module-host/extension-module-host.component').then(
        (m) => m.ExtensionModuleHostComponent
      ),
    canActivate: [oipAuthGuard]
  };
}

/**
 * Switch for a built-in route.
 *
 * - `true` enables the route under its default path.
 * - `false` disables it.
 * - a string enables it under that path, which must keep the same route parameters as the default.
 */
export type OipRouteToggle = boolean | string;

/**
 * Built-in routes provided by oip-common. Every one of them is enabled by default; disable the ones
 * a host application does not expose.
 */
export interface OipRouteFeatures {
  /** Access denied page. Default path: `access`. */
  access?: OipRouteToggle;
  /** Authentication error page. Default path: `error`. */
  error?: OipRouteToggle;
  /** Current user profile. Default path: `profile`. */
  profile?: OipRouteToggle;
  /** Application configuration. Default path: `config`. */
  config?: OipRouteToggle;
  /** Registered applications, administrators only. Default path: `applications`. */
  applications?: OipRouteToggle;
  /** Module registry, administrators only. Default path: `modules`. */
  modules?: OipRouteToggle;
  /** Discussion module. Default path: `discussion/:id`. */
  discussion?: OipRouteToggle;
  /** Database migration module. Default path: `db-migration/:id`. */
  dbMigration?: OipRouteToggle;
  /** Iframe module host. Default path: `iframe-module/:id`. */
  iframeModule?: OipRouteToggle;
  /** Extension module host. Default path: `extensions/:extensionKey/:id`. */
  extensions?: OipRouteToggle;
}

/**
 * Options accepted by {@link provideOipRoutes}.
 */
export interface OipRoutesOptions {
  /** Application specific routes rendered inside the shell. */
  children?: Routes;
  /** Which built-in routes to register, and under which paths. All of them are on by default. */
  features?: OipRouteFeatures;
  /** Shell component wrapping every child route. Default: {@link AppLayoutComponent}. */
  layout?: Type<unknown>;
  /** Routes registered outside the shell, before the not found handling. */
  rootRoutes?: Routes;
  /** Path of the not found page. Default: `notfound`. */
  notFoundPath?: string;
  /** Path of the unauthorized page. Default: `unauthorized`. */
  unauthorizedPath?: string;
  /**
   * Whether to append the `**` route redirecting to the not found page.
   * Disable it when the host application registers its own catch-all after these routes.
   * Default: `true`.
   */
  wildcard?: boolean;
}

function builtInRoute(toggle: OipRouteToggle | undefined, factory: (path?: string) => Route, into: Routes): void {
  if (toggle === false) {
    return;
  }
  into.push(typeof toggle === 'string' ? factory(toggle) : factory());
}

/**
 * Builds the standard oip route tree: an authenticated shell holding the application routes and the
 * built-in pages, followed by the unauthorized, not found and catch-all routes.
 *
 * The returned array is ordered so that the `**` route stays last; concatenating anything after it
 * makes those routes unreachable.
 *
 * @example
 * export const appRoutes = provideOipRoutes({
 *   children: [
 *     {
 *       path: 'dashboard/:id',
 *       loadComponent: () => import('./dashboard.component').then((m) => m.DashboardComponent),
 *       canActivate: [oipAuthGuard]
 *     }
 *   ],
 *   features: { dbMigration: 'legacy-migration/:id', modules: false }
 * });
 */
export function provideOipRoutes(options: OipRoutesOptions = {}): Routes {
  const features = options.features ?? {};
  const children: Routes = [...(options.children ?? [])];

  builtInRoute(features.access, oipAccessRoute, children);
  builtInRoute(features.error, oipErrorRoute, children);
  builtInRoute(features.profile, oipProfileRoute, children);
  builtInRoute(features.config, oipConfigRoute, children);
  builtInRoute(features.applications, oipApplicationsRoute, children);
  builtInRoute(features.modules, oipModulesRoute, children);
  builtInRoute(features.discussion, oipDiscussionRoute, children);
  builtInRoute(features.dbMigration, oipDbMigrationRoute, children);
  builtInRoute(features.iframeModule, oipIframeModuleRoute, children);
  builtInRoute(features.extensions, oipExtensionsRoute, children);

  const notFoundPath = options.notFoundPath ?? 'notfound';

  const routes: Routes = [
    {
      path: '',
      component: options.layout ?? AppLayoutComponent,
      canActivate: [oipAuthGuard],
      canActivateChild: [moduleAccessGuard],
      children
    },
    {
      path: options.unauthorizedPath ?? 'unauthorized',
      loadComponent: () =>
        import('../components/auth/unauthorized/unauthorized.component').then((m) => m.UnauthorizedComponent)
    },
    ...(options.rootRoutes ?? []),
    { path: notFoundPath, component: NotfoundComponent }
  ];

  if (options.wildcard !== false) {
    routes.push({ path: '**', redirectTo: `/${notFoundPath}` });
  }

  return routes;
}
