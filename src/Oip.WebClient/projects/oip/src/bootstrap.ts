import { enableProdMode } from '@angular/core';
import { environment } from './environments/environment';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app.config';

/** Starts the Angular application. */
export function bootstrap(): Promise<unknown> {
  if (environment.production) {
    enableProdMode();
  }

  return bootstrapApplication(AppComponent, appConfig);
}
