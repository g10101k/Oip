import { ApplicationConfig } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import { defaultTheme, provideAppInfo, provideAppThemes, provideOip } from 'oip-common';
import packageJson from '../../../package.json';
import { ProductService } from './app/service/product.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideOip(),
    provideAppInfo({version: packageJson.version}),
    provideAppThemes(defaultTheme, {mode: 'replaceDefaults'}),
    ProductService,
    provideRouter(
      appRoutes,
      withInMemoryScrolling({anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled'}),
      withEnabledBlockingInitialNavigation()
    ),
    provideAnimationsAsync(),
    providePrimeNG({theme: {preset: Aura, options: {darkModeSelector: '.app-dark'}}})
  ]
};
