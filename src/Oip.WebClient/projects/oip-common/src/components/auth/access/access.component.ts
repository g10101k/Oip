import { Component, inject } from '@angular/core';
import { Location } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { LogoComponent } from '../../logo/logo.component';
import { provideTranslations } from '../../../helpers/l10n.helper';

import en from './l10n/access.en.json';
import ru from './l10n/access.ru.json';

@Component({
  selector: 'app-access',
  template: ` <main class="flex min-h-[calc(100dvh-8.5rem)] items-center justify-center overflow-hidden px-4 py-12">
    <div class="flex max-w-xl flex-col items-center text-center break-words">
      <div
        class="flex select-none items-center font-bold leading-none tracking-tight text-primary text-[clamp(6rem,24vw,11rem)]"
        role="img"
        [attr.aria-label]="'access.errorCode' | translate">
        <span aria-hidden="true">4</span>
        <span
          class="relative mx-[0.04em] flex size-[0.74em] items-center justify-center rounded-full border-[0.1em] border-current"
          style="background: color-mix(in srgb, var(--primary-color), transparent 90%)"
          aria-hidden="true">
          <logo style="--logo-max-width: 0.4em; --logo-max-height: 0.4em" class="w-full" />
          <span
            class="absolute -right-[0.14em] -bottom-[0.14em] flex size-[0.3em] items-center justify-center rounded-full bg-primary text-primary-contrast ring-[0.04em] ring-surface-0 dark:ring-surface-950">
            <i class="pi pi-lock !text-[0.13em]"></i>
          </span>
        </span>
        <span aria-hidden="true">3</span>
      </div>
      <h1 class="mt-8 mb-3 text-3xl font-bold text-surface-900 dark:text-surface-0 lg:text-4xl">
        {{ 'access.title' | translate }}
      </h1>
      <p class="mb-8 max-w-md text-surface-600 dark:text-surface-200">{{ 'access.message' | translate }}</p>
      <div class="flex flex-wrap justify-center gap-3">
        <p-button
          id="oip-app-access-go-back-button"
          icon="pi pi-arrow-left"
          severity="secondary"
          [outlined]="true"
          [label]="'access.back' | translate"
          (onClick)="goBack()" />
        <p-button
          id="oip-app-access-go-to-dashboard-button"
          icon="pi pi-home"
          routerLink="/"
          [label]="'access.button' | translate" />
      </div>
    </div>
  </main>`,
  imports: [RouterLink, Button, TranslatePipe, LogoComponent],
  standalone: true
})
export class AccessComponent {
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
