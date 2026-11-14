import { Component, inject } from '@angular/core';
import { Location } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { LogoComponent } from '../logo/logo.component';
import { Button } from 'primeng/button';
import { AppFloatingConfiguratorComponent } from '../app-floating-configurator/app-floating-configurator.component';
import { TranslatePipe } from '@ngx-translate/core';
import { provideTranslations } from '../../helpers/l10n.helper';

import en from './l10n/notfound.en.json';
import ru from './l10n/notfound.ru.json';

@Component({
  selector: 'app-notfound',
  template: ` <app-floating-configurator />
    <main class="flex min-h-screen items-center justify-center overflow-hidden px-4 py-20">
      <div class="flex max-w-xl flex-col items-center text-center break-words">
        <div
          class="flex select-none items-center font-bold leading-none tracking-tight text-primary text-[clamp(6rem,24vw,11rem)]"
          role="img"
          [attr.aria-label]="'notfound.errorCode' | translate">
          <span aria-hidden="true">4</span>
          <span
            class="mx-[0.04em] flex size-[0.74em] items-center justify-center rounded-full border-[0.1em] border-current"
            style="background: color-mix(in srgb, var(--primary-color), transparent 90%)"
            aria-hidden="true">
            <logo style="--logo-max-width: 0.4em; --logo-max-height: 0.4em" class="w-full" />
          </span>
          <span aria-hidden="true">4</span>
        </div>
        <h1 class="mt-8 mb-3 text-3xl font-bold text-surface-900 dark:text-surface-0 lg:text-4xl">
          {{ 'notfound.title' | translate }}
        </h1>
        <p class="mb-8 max-w-md text-surface-600 dark:text-surface-200">{{ 'notfound.description' | translate }}</p>
        <div class="flex flex-wrap justify-center gap-3">
          <p-button
            id="oip-app-notfound-go-back-button"
            icon="pi pi-arrow-left"
            severity="secondary"
            [outlined]="true"
            [label]="'notfound.back' | translate"
            (onClick)="goBack()" />
          <p-button
            id="oip-app-notfound-go-to-home-button"
            icon="pi pi-home"
            routerLink="/"
            [label]="'notfound.button' | translate" />
        </div>
      </div>
    </main>`,
  imports: [RouterLink, LogoComponent, Button, AppFloatingConfiguratorComponent, TranslatePipe],
  standalone: true
})
export class NotfoundComponent {
  private readonly translations = provideTranslations({ en, ru });
  private readonly location = inject(Location);
  private readonly router = inject(Router);

  /**
   * Returns to the previous page, or to the home page when there is no history.
   */
  goBack(): void {
    if (window.history.length > 1) {
      this.location.back();
    } else {
      void this.router.navigateByUrl('/');
    }
  }
}
