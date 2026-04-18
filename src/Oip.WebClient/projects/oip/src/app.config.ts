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
        },

        tooltip: {
          colorScheme: {
            light: {
              root: {
                background: 'var(--color-light-tooltip)',
                borderRadius: '0.7rem'
              }
            },
            dark: {
              root: {
                background: 'var(--color-light-tooltip)',
                borderRadius: '0.7rem'
              }
            }
          }
        },

        datatable: {
          colorScheme: {
            light: {
              root: {
                borderColor: 'var(--color-light-green)'
              },
              header: {
                background: 'var(--color-light-green)',
                color: 'var(--color-dark)'
              },
              headerCell: {
                gap: '0px',
                background: 'var(--color-light-green)',
                color: 'var(--color-dark)',
                borderColor: 'var(--color-light-green)'
              }
            },
            dark: {
              root: {
                borderColor: 'var(--color-gray)'
              },
              header: {
                background: 'var(--color-gray)',
                color: 'var(--color-dark)'
              },
              headerCell: {
                background: 'var(--color-gray)',
                color: 'var(--color-dark)',
                borderColor: 'var(--color-gray)'
              }
            }
          }
        }
      }
    }),
    primaryColors: {
      rose: { 500: '#f43f5e', 600: '#e11d48', 700: '#be123c' }
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
