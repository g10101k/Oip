import { loadRemoteModule } from '@angular-architects/module-federation';
import { Routes } from '@angular/router';
import { APP_ROUTES, APP_ROUTES_END } from '../app.routes';
import { CustomManifest } from 'shared-lib';
import { AuthGuardService } from "../services/auth.service";
import { inject } from "@angular/core";

export function buildRoutes(manifest: CustomManifest): Routes {
  const lazyRoutes: Routes = Object.keys(manifest).map(key => {
    const entry = manifest[key];
    return {
      path: entry.routePath,
      providers: [{provide: 'BASE_URL', useValue: entry.baseUrl, deps: []}],
      canActivate: [() => inject(AuthGuardService).canActivate()],
      loadChildren: () =>
        loadRemoteModule({
          type: 'manifest',
          remoteName: key,
          exposedModule: entry.exposedModule,
        }).then(m => {
          return m[entry.ngModuleName]
        })
    }
  });

  return [...APP_ROUTES, ...lazyRoutes, ...APP_ROUTES_END];
}
