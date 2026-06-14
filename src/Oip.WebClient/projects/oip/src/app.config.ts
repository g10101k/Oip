import { ApplicationConfig, inject, provideAppInitializer } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import { ApplicationsApi, provideAppThemes, provideOip } from 'oip-common';
import { ProductService } from './app/service/product.service';
import { appTheme } from './app.theme';
import { FederatedRemoteRegistryService } from './app/services/federated-remotes/federated-remote-registry.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideOip(),
    provideAppThemes(appTheme, { mode: 'replaceDefaults' }),
    ApplicationsApi,
    ProductService,
    provideAppInitializer(() => inject(FederatedRemoteRegistryService).registerRoutes()),
    provideRouter(appRoutes, withInMemoryScrolling({anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled'}),
      withEnabledBlockingInitialNavigation()
    ),
    provideAnimationsAsync(),
    providePrimeNG({theme: {preset: Aura, options: {darkModeSelector: '.app-dark'}}})
  ]
};
