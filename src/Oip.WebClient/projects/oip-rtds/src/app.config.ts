import { ApplicationConfig } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import Aura from '@primeng/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import { provideAppThemes, provideOip } from 'oip-common';
import { appTheme } from "../../oip/src/app.theme";

export const appConfig: ApplicationConfig = {
  providers: [
    provideOip(),
    provideAppThemes(appTheme, { mode: 'replaceDefaults' }),
    provideRouter(appRoutes, withInMemoryScrolling({anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled'}),
      withEnabledBlockingInitialNavigation()
    ),
    provideAnimationsAsync(),
    providePrimeNG({theme: {preset: Aura, options: {darkModeSelector: '.app-dark'}}})
  ]
};
