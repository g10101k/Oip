import { initFederation } from '@angular-architects/native-federation';

initFederation()
  .catch(() => undefined)
  .then(_ => import('./bootstrap'))
  .catch(err => console.error(err));
