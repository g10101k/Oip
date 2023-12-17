import {loadRemoteModule} from '@angular-architects/module-federation';
import {Routes} from '@angular/router';
import {APP_ROUTES, APP_ROUTES_END} from '../app.routes';
import {CustomManifest} from 'shared-lib';
import {AuthLibService} from "auth-lib";

export function buildRoutes(options: CustomManifest): Routes {
  const lazyRoutes: Routes = Object.keys(options).map(key => {

    const entry = options[key];
    return {
      path: entry.routePath,
      providers: [{ provide: 'BASE_URL', useValue: entry.baseUrl, deps: [] }],
      loadChildren: () =>
        loadRemoteModule({
          type: 'manifest',
          remoteName: key,
          exposedModule: entry.exposedModule
        }).then(m => {
          return m[entry.ngModuleName]
        })
    }
  });

  return [...APP_ROUTES, ...lazyRoutes, ...APP_ROUTES_END];
}
