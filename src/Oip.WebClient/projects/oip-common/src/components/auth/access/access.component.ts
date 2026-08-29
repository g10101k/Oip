import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { TranslatePipe } from '@ngx-translate/core';
import { provideTranslations } from '../../../helpers/l10n.helper';

import en from './l10n/access.en.json';
import ru from './l10n/access.ru.json';

@Component({
  selector: 'app-access',
  template: `<div
    class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
    <div class="flex flex-column align-items-center justify-content-center">
      <img alt="Sakai logo" class="mb-5 w-6rem flex-shrink-0" src="assets/demo/images/access/logo-orange.svg" />
      <div
        style="border-radius:56px; padding:0.3rem; background: linear-gradient(180deg, rgba(247, 149, 48, 0.4) 10%, rgba(247, 149, 48, 0) 30%);">
        <div
          class="w-full surface-card py-8 px-5 sm:px-8 flex flex-column align-items-center"
          style="border-radius:53px">
          <div class="grid flex-column align-items-center">
            <div
              class="flex justify-content-center align-items-center bg-orange-500 border-circle"
              style="width:3.2rem; height:3.2rem;">
              <i class="text-50 pi pi-fw pi-lock text-2xl"></i>
            </div>
            <h1 class="text-900 font-bold text-4xl lg:text-5xl mb-2">{{ 'access.title' | translate }}</h1>
            <span class="text-600 mb-5">{{ 'access.message' | translate }}</span>
            <img alt="Access denied" class="mb-5" src="assets/demo/images/access/asset-access.svg" width="80%" />
            <p-button
              class="p-button-text"
              icon="pi pi-arrow-left"
              id="oip-app-access-go-to-dashboard-button"
              pButton
              pRipple
              [label]="'access.button' | translate"
              [routerLink]="['/']"></p-button>
          </div>
        </div>
      </div>
    </div>
  </div>`,
  imports: [RouterLink, ButtonModule, RippleModule, TranslatePipe],
  standalone: true
})
export class AccessComponent {
  private readonly translations = provideTranslations({ en, ru });
}
