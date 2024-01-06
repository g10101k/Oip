import { loadManifest } from '@angular-architects/module-federation';

loadManifest("/api/module-federation/get-manifest")
  .catch(err => console.error(err))
  .then(_ => import('./bootstrap'))
  .catch(err => console.error(err));
