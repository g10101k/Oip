import { provideHttpClient, withFetch } from '@angular/common/http';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import { AuthConfigModule, AuthGuardService, BaseDataService, SecurityDataService, UserService } from "oip-common";
import { LocationStrategy, PathLocationStrategy } from "@angular/common";
import { CountryService } from "./app/demo/service/country.service";
import { CustomerService } from "./app/demo/service/customer.service";
import { EventService } from "./app/demo/service/event.service";
import { IconService } from "./app/demo/service/icon.service";
import { NodeService } from "./app/demo/service/node.service";
import { PhotoService } from "./app/demo/service/photo.service";
import { ProductService } from "./app/demo/service/product.service";
import { MessageService } from "primeng/api";

export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(AuthConfigModule),
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    CountryService,
    CustomerService,
    EventService,
    IconService,
    NodeService,
    PhotoService,
    ProductService,
    AuthGuardService,
    MessageService,
    SecurityDataService,
    BaseDataService,
    UserService,
    provideRouter(appRoutes, withInMemoryScrolling({
      anchorScrolling: 'enabled',
      scrollPositionRestoration: 'enabled'
    }), withEnabledBlockingInitialNavigation()),
    provideHttpClient(withFetch()),
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

