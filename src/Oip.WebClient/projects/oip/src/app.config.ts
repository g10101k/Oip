import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import {

  BaseDataService,
  SecurityDataService,
  UserService,
  langIntercept,
  ConfigService
} from "oip-common";
import { LocationStrategy, PathLocationStrategy } from "@angular/common";
import { ProductService } from "./app/service/product.service";
import { MessageService } from "primeng/api";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { AutoRefreshTokenService, provideKeycloak, UserActivityService, withAutoRefreshToken } from "keycloak-angular";

const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (http: HttpClient) =>
  new TranslateHttpLoader(http);

export const appConfig: ApplicationConfig = {
  providers: [
    ConfigService,
    provideHttpClient(withInterceptors([langIntercept]), withFetch()),
    provideKeycloak({
      config: {
        url: 'https://localhost:8443',
        realm: 'oip',
        clientId: 'oip-client'
      },
      initOptions: {
        onLoad: 'check-sso',
        silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html'
      },
      features: [
        withAutoRefreshToken({
          onInactivityTimeout: 'logout',
          sessionTimeout: 60000
        })
      ],
      providers: [AutoRefreshTokenService, UserActivityService]
    }),
    provideZoneChangeDetection({ eventCoalescing: true }),

    { provide: LocationStrategy, useClass: PathLocationStrategy },
    ProductService,
    MessageService,
    SecurityDataService,
    BaseDataService,
    UserService,
    importProvidersFrom([TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: httpLoaderFactory,
        deps: [HttpClient],
      },
    })]),

    provideRouter(appRoutes, withInMemoryScrolling({
      anchorScrolling: 'enabled',
      scrollPositionRestoration: 'enabled'
    }), withEnabledBlockingInitialNavigation()),
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

