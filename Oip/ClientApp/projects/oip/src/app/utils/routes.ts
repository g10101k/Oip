import {loadRemoteModule} from '@angular-architects/module-federation';
import {Routes} from '@angular/router';
import {APP_ROUTES, APP_ROUTES_END} from '../app.routes';
import {CustomManifest} from './config';

export function buildRoutes(options: CustomManifest): Routes {
  const lazyRoutes: Routes = Object.keys(options).map(key => {
    const entry = options[key];
    return {
      path: entry.routePath,
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
