import { LocationStrategy, PathLocationStrategy } from '@angular/common';
import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import Aura from '@primeng/themes/aura';
import { AbstractSecurityStorage, authInterceptor, provideAuth, StsConfigLoader } from 'angular-auth-oidc-client';
import {
  AuthGuardService,
  BaseDataService,
  DEFAULT_OIP_FRONTEND_CONFIG,
  httpLoaderAuthFactory,
  KeycloakSecurityService,
  langIntercept,
  OIP_FRONTEND_CONFIG,
  SecurityDataService,
  SecurityService,
  SecurityStorageService,
  UserService
} from 'oip-common';
import { MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { environment } from '../environments/environment';
import { appRoutes } from './app.routes';

const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (http: HttpClient) =>
  new TranslateHttpLoader(http);

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor(), langIntercept]), withFetch()),
    provideAuth({
      loader: {
        provide: StsConfigLoader,
        useFactory: httpLoaderAuthFactory,
        deps: [HttpClient]
      }
    }),
    {
      provide: OIP_FRONTEND_CONFIG,
      useValue: {
        ...DEFAULT_OIP_FRONTEND_CONFIG,
        ...environment.frontend
      }
    },
    { provide: AbstractSecurityStorage, useClass: SecurityStorageService },
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    { provide: SecurityService, useClass: KeycloakSecurityService },
    AuthGuardService,
    MessageService,
    SecurityDataService,
    BaseDataService,
    UserService,
    importProvidersFrom([
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: httpLoaderFactory,
          deps: [HttpClient]
        }
      })
    ]),
    provideRouter(
      appRoutes,
      withInMemoryScrolling({
        anchorScrolling: 'enabled',
        scrollPositionRestoration: 'enabled'
      }),
      withEnabledBlockingInitialNavigation()
    ),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: '.app-dark'
        }
      }
    })
  ]
};
