import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { ModuleConfigComponent } from "./config/module-config.component";
import { AppLayoutComponent } from "./layout/app.layout.component";
import { DashboardComponent } from "./demo/components/dashboard/dashboard.component";

export const APP_ROUTES: Routes = [
  {
    path: '', loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule)
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
