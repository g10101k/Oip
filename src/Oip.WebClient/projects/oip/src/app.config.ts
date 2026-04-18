import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import {
  AppThemePreset,
  AuthGuardService,
  BaseDataService,
  DEFAULT_OIP_FRONTEND_CONFIG,
  OIP_FRONTEND_CONFIG,
  SecurityDataService,
  SecurityStorageService,
  UserService,
  langIntercept,
  httpLoaderAuthFactory,
  provideAppThemes,
  SecurityService,
  KeycloakSecurityService,
  NotificationService
} from 'oip-common';
import { LocationStrategy, PathLocationStrategy } from '@angular/common';
import { ProductService } from './app/service/product.service';
import { MessageService } from 'primeng/api';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { AbstractSecurityStorage, authInterceptor, provideAuth, StsConfigLoader } from 'angular-auth-oidc-client';
import { environment } from './environments/environment';
import { definePreset } from '@primeng/themes';
import { primitive as AuraPrimitive } from '@primeng/themes/aura/base';

const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (http: HttpClient) =>
  new TranslateHttpLoader(http);

const customThemes: AppThemePreset[] = [
  {
    id: 'Corporate',
    label: 'Corporate',
    preset: definePreset(Aura, {
      components: {
        toolbar: {
          colorScheme: {
            light: {
              root: {
                borderRadius: '0.7rem'
              }
            },
            dark: {
              root: {
                borderRadius: '0.7rem'
              }
            }
          }
        }
      }
    }),
    primaryColors: {
      rose: AuraPrimitive['rose']
    }
  }
];


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
    provideAppThemes(customThemes, { mode: 'replaceDefaults' }),
    { provide: AbstractSecurityStorage, useClass: SecurityStorageService },
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    { provide: SecurityService, useClass: KeycloakSecurityService },
    {
      provide: OIP_FRONTEND_CONFIG,
      useValue: {
        ...DEFAULT_OIP_FRONTEND_CONFIG,
        ...environment.frontend
      }
    },
    ProductService,
    AuthGuardService,
    MessageService,
    SecurityDataService,
    BaseDataService,
    UserService,
    NotificationService,
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
