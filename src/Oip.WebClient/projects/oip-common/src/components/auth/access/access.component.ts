import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { LogoComponent } from '../../logo/logo.component';
import { provideTranslations } from '../../../helpers/l10n.helper';

import en from './l10n/access.en.json';
import ru from './l10n/access.ru.json';

@Component({
  selector: 'app-access',
  template: ` <div class="flex min-h-screen items-center justify-center px-4 py-20">
    <div
      class="max-w-full rounded-[56px] p-[0.3rem]"
      style="background: linear-gradient(180deg, color-mix(in srgb, var(--primary-color), transparent 60%) 10%, var(--surface-ground) 30%)">
      <div
        class="flex w-full flex-col items-center rounded-[53px] bg-surface-0 px-8 py-20 text-center break-words dark:bg-surface-900 sm:px-20">
        <logo class="logo-error-page" />
        <i class="pi pi-lock text-primary !text-3xl" aria-hidden="true"></i>
        <h1 class="max-w-full text-surface-900 dark:text-surface-0 font-bold text-3xl lg:text-5xl mb-2">
          {{ 'access.title' | translate }}
        </h1>
        <div class="max-w-full text-surface-600 dark:text-surface-200 mb-8">{{ 'access.message' | translate }}</div>
        <p-button id="oip-app-access-go-to-dashboard-button" routerLink="/" [label]="'access.button' | translate" />
      </div>
    </div>
  </div>`,
  imports: [RouterLink, Button, TranslatePipe, LogoComponent],
  standalone: true
})
export class AccessComponent {
  private readonly translations = provideTranslations({ en, ru });
}
