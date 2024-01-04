import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { NotfoundComponent } from './demo/components/notfound/notfound.component';
import { ConfigComponent } from "./config/config.component";
import { AppLayoutComponent } from "./layout/app.layout.component";

export const APP_ROUTES: Routes = [
  {
    path: '',
    component: AppLayoutComponent,
    children: [
      { path: '', loadChildren: () => import('./demo/components/dashboard/dashboard.module').then(m => m.DashboardModule) },

    ]
  },
  {
    path: 'config',
    component: ConfigComponent,
    pathMatch: 'full',
  }
];

export const APP_ROUTES_END: Routes = [

  {
    path: '**',
    component: NotfoundComponent,
  },
]
