import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { provideTranslations } from '../../helpers/l10n.helper';

import en from './l10n/no-modules.en.json';
import ru from './l10n/no-modules.ru.json';

/**
 * Landing page shown when the user has no module instance to open.
 *
 * Rendered inside the shell on purpose: the menu and the top bar stay available, so an
 * administrator can keep working while a regular user sees why the page is empty.
 */
@Component({
  selector: 'app-no-modules',
  template: `<div class="flex flex-col items-center justify-center text-center gap-4 py-20">
    <i class="pi pi-inbox text-5xl text-surface-400"></i>
    <h1 class="text-surface-900 dark:text-surface-0 font-bold text-2xl lg:text-3xl m-0">
      {{ 'noModules.title' | translate }}
    </h1>
    <div class="text-surface-600 dark:text-surface-200 max-w-2xl">{{ 'noModules.description' | translate }}</div>
  </div>`,
  imports: [TranslateModule],
  standalone: true
})
export class NoModulesComponent {
  private readonly translations = provideTranslations({ en, ru });
}
