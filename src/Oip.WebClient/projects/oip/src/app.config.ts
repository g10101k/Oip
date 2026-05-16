import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import {
  AuthGuardService,
  UserService,
  langIntercept,
  provideAppThemes,
  SecurityService,
  BffSecurityService,
  NotificationService,
  UserProfileApi,
  SecurityApi,
  NotificationApi
} from 'oip-common';
import { LocationStrategy, PathLocationStrategy } from '@angular/common';
import { ProductService } from './app/service/product.service';
import { MessageService } from 'primeng/api';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { appTheme } from "./app.theme";

const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (http: HttpClient) =>
  new TranslateHttpLoader(http);

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([langIntercept]), withFetch()),
    provideAppThemes(appTheme, { mode: 'replaceDefaults' }),
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    { provide: SecurityService, useClass: BffSecurityService },
    UserProfileApi,
    SecurityApi,
    ProductService,
    AuthGuardService,
    MessageService,
    UserService,
    NotificationService,
    NotificationApi,
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
