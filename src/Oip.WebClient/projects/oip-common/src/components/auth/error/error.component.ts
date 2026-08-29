import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { provideTranslations } from '../../../helpers/l10n.helper';

import en from './l10n/error.en.json';
import ru from './l10n/error.ru.json';

@Component({
  selector: 'app-error',
  template: `<div
    class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
    <div class="flex flex-column align-items-center justify-content-center">
      <img alt="Sakai logo" class="mb-5 w-6rem flex-shrink-0" src="assets/demo/images/error/logo-error.svg" />
      <div
        style="border-radius:56px; padding:0.3rem; background: linear-gradient(180deg, rgba(233, 30, 99, 0.4) 10%, rgba(33, 150, 243, 0) 30%);">
        <div
          class="w-full surface-card py-8 px-5 sm:px-8 flex flex-column align-items-center"
          style="border-radius:53px">
          <div class="grid flex flex-column align-items-center">
            <div
              class="flex justify-content-center align-items-center bg-pink-500 border-circle"
              style="height:3.2rem; width:3.2rem;">
              <i class="pi pi-fw pi-exclamation-circle text-2xl text-white"></i>
            </div>
            <h1 class="text-900 font-bold text-5xl mb-2">{{ 'error.title' | translate }}</h1>
            <span class="text-600 mb-5">{{ 'error.message' | translate }}</span>
            <img alt="Error" class="mb-5" src="assets/demo/images/error/asset-error.svg" width="80%" />
            <button
              class="p-button-text"
              icon="pi pi-arrow-left"
              id="oip-app-error-go-to-dashboard-button"
              pButton
              pRipple
              [label]="'error.button' | translate"
              [routerLink]="['/']"></button>
          </div>
        </div>
      </div>
    </div>
  </div>`,
  standalone: true,
  imports: [ButtonModule, RippleModule, RouterLink, TranslatePipe]
})
export class ErrorComponent {
  private readonly translations = provideTranslations({ en, ru });
}
