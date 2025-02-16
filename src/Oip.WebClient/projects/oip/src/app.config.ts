import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import {
  AuthGuardService,
  BaseDataService,
  SecurityDataService,
  UserService,
  langIntercept, httpLoaderAuthFactory
} from "oip-common";
import { LocationStrategy, PathLocationStrategy } from "@angular/common";
import { CountryService } from "./app/demo/service/country.service";
import { IconService } from "./app/demo/service/icon.service";
import { ProductService } from "./app/demo/service/product.service";
import { MessageService } from "primeng/api";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { AbstractSecurityStorage, authInterceptor, provideAuth, StsConfigLoader } from "angular-auth-oidc-client";
import { SecurityStorageService } from "../../oip-common/src/services/security-storage.service";

const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (http: HttpClient) =>
  new TranslateHttpLoader(http);

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor(), langIntercept]), withFetch()),
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    { provide: AbstractSecurityStorage, useClass: SecurityStorageService },
    CountryService,
    IconService,
    ProductService,
    AuthGuardService,
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
    provideAuth({
      loader: {
        provide: StsConfigLoader,
        useFactory: httpLoaderAuthFactory,
        deps: [HttpClient],
      },
    }),
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

