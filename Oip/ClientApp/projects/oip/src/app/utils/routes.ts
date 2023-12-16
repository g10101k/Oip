import {loadRemoteModule} from '@angular-architects/module-federation';
import {Routes} from '@angular/router';
import {APP_ROUTES, APP_ROUTES_END} from '../app.routes';
import {CustomManifest} from './config';
import {HttpClient} from "@angular/common/http";

export function buildRoutes(options: CustomManifest): Routes {
  let data  = ''

  fetch('./mf.manifest.json.')
    .then(function(response) {
      return response.json();
    })
    .then(function(myJson) {

      data=myJson
      console.log(data)
    });

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
