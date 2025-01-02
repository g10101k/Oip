import { ApplicationConfig, importProvidersFrom, inject } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import {
  ActivatedRouteSnapshot, CanActivateChildFn,
  CanActivateFn,
  provideRouter, Router,
  RouterModule,
  RouterStateSnapshot, UrlTree
} from "@angular/router";
import { AppLayoutComponent } from "./layout/app.layout.component";
import { AuthGuardService } from "./services/auth.service";
import { NotfoundComponent } from "./demo/components/notfound/notfound.component";
import { Routes } from '@angular/router';
import { provideHttpClient } from "@angular/common/http";
import { LogLevel, OidcSecurityService, provideAuth } from "angular-auth-oidc-client";
import { catchError, map, take } from "rxjs/operators";
import {
  APP_INITIALIZER,
  EnvironmentProviders,
  makeEnvironmentProviders,
  Provider,
} from '@angular/core';
import { definePreset } from '@primeng/themes';
import Aura from '@primeng/themes/aura';

export const routes: Routes = [{
  path: '', component: AppLayoutComponent,
  children: [
    {
      path: 'dashboard/:id',
      loadComponent: () => import('./demo/components/dashboard/dashboard.component').then(m => m.DashboardComponent),
      canActivate: [() => inject(AuthGuardService).canActivate()],
    },
    {
      path: 'uikit',
      loadChildren: () => import('./demo/components/uikit/uikit.module').then(m => m.UIkitModule),
      canActivate: [() => inject(AuthGuardService).canActivate()],
    },
    {
      path: 'utilities',
      loadChildren: () => import('./demo/components/utilities/utilities.module').then(m => m.UtilitiesModule),
      canActivate: [() => inject(AuthGuardService).canActivate()],
    },
    {
      path: 'documentation',
      loadChildren: () => import('./demo/components/documentation/documentation.module').then(m => m.DocumentationModule),
      canActivate: [() => inject(AuthGuardService).canActivate()],
    },
    {
      path: 'pages',
      loadChildren: () => import('./demo/components/pages/pages.module').then(m => m.PagesModule),
      canActivate: [() => inject(AuthGuardService).canActivate()],
    },
    {
      path: 'config',
      loadChildren: () => import('./layout/config/app.config.module').then(m => m.AppConfigModule),
      canActivate: [() => inject(AuthGuardService).canActivate()]
    },
    {
      path: 'weather/:id',
      loadChildren: () => import('./demo/components/weather/weather.module').then(m => m.WeatherModule),
      canActivate: [() => inject(AuthGuardService).canActivate()]
    },

    {
      path: 'error',
      loadChildren: () => import('./demo/components/auth/error/error.module').then(m => m.ErrorModule)
    }
  ]
},
  {
    path: 'unauthorized',
    loadChildren: () => import('./demo/components/auth/unauthorized/unauthorized.module').then(m => m.UnauthorizedModule)
  },
  {
    path: 'blocks',
    loadChildren: () => import('./demo/components/primeblocks/primeblocks.module').then(m => m.PrimeBlocksModule)
  },
  { path: 'auth', loadChildren: () => import('./demo/components/auth/auth-routing.module').then(m => m.AuthRouting) },
  {
    path: 'landing',
    loadChildren: () => import('./demo/components/landing/landing.module').then(m => m.LandingModule)
  },
  {
    path: 'notfound',
    loadChildren: () => import('./demo/components/notfound/notfound.module').then(m => m.NotfoundModule)
  },
  { path: '**', redirectTo: '/notfound' }];



export function provideAuthGuard(): EnvironmentProviders {
  const providers: Provider[] = [{ provide: AbstractAuthGuardService, useClass: AuthGuardService }];
  return makeEnvironmentProviders(providers);
}


import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { MsgService } from "common";
import { MessageService } from "primeng/api";

export abstract class AbstractAuthGuardService {
  abstract canActivate(): Observable<boolean | UrlTree> ;
}



export const Noir = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{surface.50}',
      100: '{surface.100}',
      200: '{surface.200}',
      300: '{surface.300}',
      400: '{surface.400}',
      500: '{surface.500}',
      600: '{surface.600}',
      700: '{surface.700}',
      800: '{surface.800}',
      900: '{surface.900}',
      950: '{surface.950}'
    },
    colorScheme: {
      light: {
        primary: {
          color: '{primary.950}',
          contrastColor: '#ffffff',
          hoverColor: '{primary.800}',
          activeColor: '{primary.700}'
        },
        highlight: {
          background: '{primary.950}',
          focusBackground: '{primary.700}',
          color: '#ffffff',
          focusColor: '#ffffff'
        }
      },
      dark: {
        primary: {
          color: '{primary.50}',
          contrastColor: '{primary.950}',
          hoverColor: '{primary.200}',
          activeColor: '{primary.300}'
        },
        highlight: {
          background: '{primary.50}',
          focusBackground: '{primary.300}',
          color: '{primary.950}',
          focusColor: '{primary.950}'
        }
      }
    }
  }
});


export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideAuth({
      config: {
        authority: 'https://s-gbt-wsn-00010:8443/realms/oip',
        redirectUrl: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        clientId: 'oip-client',
        scope: 'openid profile email offline_access roles',
        responseType: 'code',
        silentRenew: true,
        useRefreshToken: true,
        logLevel: LogLevel.Debug,
      },
    }),
    provideRouter(routes),
    provideAnimationsAsync(),
    providePrimeNG({ theme: Noir, ripple: false, inputStyle: 'outlined' }),
    { provide: AuthGuardService, useClass: AuthGuardService },
    { provide: MsgService, useClass: MsgService },
    { provide: MessageService, useClass: MessageService },
  ]
};
