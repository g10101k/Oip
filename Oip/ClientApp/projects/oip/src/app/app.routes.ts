import { Routes } from '@angular/router';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { ModuleConfigComponent } from "./config/module-config.component";

export const APP_ROUTES: Routes = [
  {
    path: '',
    loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'block-marketing',
    loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule)
  },


  {
    path: 'config',
    component: ModuleConfigComponent,
    pathMatch: 'full',
  }
];

export const APP_ROUTES_END: Routes = [

  {
    path: '**',
    component: NotfoundComponent,
  },
]
