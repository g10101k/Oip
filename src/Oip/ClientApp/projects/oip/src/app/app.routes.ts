import { Routes } from '@angular/router';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { ModuleConfigComponent } from "./config/module-config.component";

export const APP_ROUTES: Routes = [
  {
    path: '',
    loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'blocks',
    loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule)
  },
  {path: 'uikit', loadChildren: () => import('./demo/components/uikit/uikit.module').then(m => m.UIkitModule)},
  {
    path: 'utilities',
    loadChildren: () => import('./demo/components/utilities/utilities.module').then(m => m.UtilitiesModule)
  },
  {
    path: 'documentation',
    loadChildren: () => import('./demo/components/documentation/documentation.module').then(m => m.DocumentationModule)
  },
  {
    path: 'pages', loadChildren: () => import('./demo/components/pages/pages.module').then(m => m.PagesModule)
  },
  {path: 'auth', loadChildren: () => import('./demo/components/auth/auth.module').then(m => m.AuthModule)},
  {path: 'landing', loadChildren: () => import('./demo/components/landing/landing.module').then(m => m.LandingModule)},
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
