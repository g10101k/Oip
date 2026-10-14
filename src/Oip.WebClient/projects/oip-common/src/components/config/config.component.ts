import { Component, computed, DestroyRef, inject, LOCALE_ID, signal } from '@angular/core';
import { formatDate } from '@angular/common';
import { toSignal } from '@angular/core/rxjs-interop';
import { UserProfileComponent } from '../user-profile/user-profile.component';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { Tag } from 'primeng/tag';
import { LayoutService } from '../../services/app.layout.service';
import { UserService } from '../../services/user.service';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { RouterLink } from '@angular/router';
import { SecurityService } from '../../services/security.service';
import { MenuService } from '../../services/app.menu.service';
import { L10nService } from '../../services/l10n.service';
import { TranslatePipe } from '@ngx-translate/core';
import { provideTranslations } from '../../helpers/l10n.helper';

import en from './l10n/config.en.json';
import ru from './l10n/config.ru.json';

/** Keycloak roles every user has; they say nothing about the user, so the profile doesn't show them. */
const HIDDEN_ROLES = ['offline_access', 'uma_authorization'];
const HIDDEN_ROLE_PREFIX = 'default-roles-';

/** How often the date and time preview is refreshed. */
const PREVIEW_REFRESH_MS = 30_000;

@Component({
  selector: 'app-config',
  template: `
    <div class="flex flex-col gap-4">
      <div class="card !mb-0 flex flex-col sm:flex-row items-center gap-6">
        <user-profile />
        <div class="flex flex-col items-center sm:items-start gap-2 min-w-0 text-center sm:text-left">
          <div class="text-2xl font-semibold break-words" id="oip-app-config-user-name">
            {{ userService.userName }}
          </div>
          @if (email()) {
            <div class="text-muted-color break-all">{{ email() }}</div>
          }
          @if (roles().length) {
            <div class="flex flex-wrap justify-center sm:justify-start gap-2">
              @for (role of roles(); track role) {
                <p-tag [severity]="role === 'admin' ? 'info' : 'secondary'" [value]="role" />
              }
            </div>
          }
        </div>
      </div>

      <div class="flex flex-col lg:flex-row gap-4">
        <div class="card !mb-0 lg:flex-[3] flex flex-col gap-4 min-w-0">
          <div class="font-semibold text-xl">{{ 'config.languageAndRegion' | translate }}</div>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <label for="oip-app-config-language">{{ 'config.language' | translate }}</label>
              <p-select
                class="w-full"
                inputId="oip-app-config-language"
                optionLabel="name"
                optionValue="code"
                qa-id="oip-app-config-language-select"
                [options]="l10nService.availableLanguages"
                [(ngModel)]="selectedLanguage"
                (onChange)="changeLanguage()">
                <ng-template #selectedItem let-selectedOption>
                  <div class="flex items-center gap-2">
                    <span [class]="selectedOption.icon"></span>
                    <div>{{ selectedOption.name }}</div>
                  </div>
                </ng-template>
                <ng-template #item let-languages>
                  <div class="flex items-center gap-2">
                    <span [class]="languages.icon"></span>
                    <div>{{ languages.name }}</div>
                  </div>
                </ng-template>
              </p-select>
            </div>
            <div class="flex flex-col gap-2">
              <label for="oip-app-config-timezone">{{ 'config.timeZone' | translate }}</label>
              <p-select
                class="w-full"
                inputId="oip-app-config-timezone"
                qa-id="oip-app-config-timezone-select"
                [filter]="true"
                [options]="allTimeZones"
                [virtualScroll]="true"
                [virtualScrollItemSize]="34"
                [(ngModel)]="selectedTimeZone"
                (onChange)="changeTimeZone()" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="oip-app-config-date-format">{{ 'config.dateFormat' | translate }}</label>
              <p-select
                class="w-full"
                inputId="oip-app-config-date-format"
                qa-id="oip-app-config-date-format-select"
                [options]="dateFormats"
                [(ngModel)]="selectedDateFormat"
                (onChange)="changeDateFormat()" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="oip-app-config-time-format">{{ 'config.timeFormat' | translate }}</label>
              <p-select
                class="w-full"
                inputId="oip-app-config-time-format"
                qa-id="oip-app-config-time-format-select"
                [options]="timeFormats"
                [(ngModel)]="selectedTimeFormat"
                (onChange)="changeTimeFormat()" />
            </div>
            <div class="flex flex-col gap-2">
              <label for="oip-app-config-first-day-of-week">{{ 'config.firstDayOfWeek' | translate }}</label>
              <p-select
                class="w-full"
                inputId="oip-app-config-first-day-of-week"
                optionLabel="label"
                optionValue="value"
                qa-id="oip-app-config-first-day-of-week-select"
                [options]="weekDays()"
                [(ngModel)]="selectedFirstDayOfWeek"
                (onChange)="changeFirstDayOfWeek()" />
            </div>
            <div class="flex items-end gap-2 text-muted-color pb-2">
              <i class="pi pi-clock"></i>
              <span id="oip-app-config-now">{{ 'config.now' | translate }}: {{ nowPreview() }}</span>
            </div>
          </div>
        </div>

        @if (securityService.isAdmin()) {
          <div class="card !mb-0 lg:flex-[2] flex flex-col gap-4 min-w-0">
            <div class="font-semibold text-xl">{{ 'config.administration' | translate }}</div>
            <div class="flex items-center justify-between gap-4">
              <label class="flex flex-col gap-1" for="oip-app-config-admin-mode">
                <span>{{ 'config.showAllMenuItems' | translate }}</span>
                <span class="text-sm text-muted-color">{{ 'config.showAllMenuItemsHint' | translate }}</span>
              </label>
              <p-toggle-switch
                inputId="oip-app-config-admin-mode"
                [(ngModel)]="menuService.adminMode"
                (onChange)="onSwitchChange()" />
            </div>
            <div class="grid grid-cols-3 gap-2">
              @for (link of adminLinks; track link.path) {
                <a
                  class="flex flex-col items-center justify-center gap-2 p-3 rounded-border border border-surface text-center text-color no-underline transition-colors hover:bg-emphasis"
                  [id]="link.id"
                  [routerLink]="link.path">
                  <i [class]="link.icon + ' !text-xl'"></i>
                  <span class="text-sm">{{ link.label | translate }}</span>
                </a>
              }
            </div>
          </div>
        }
      </div>
    </div>
  `,
  imports: [UserProfileComponent, FormsModule, Select, Tag, TranslatePipe, ToggleSwitchModule, RouterLink]
})
export class ConfigComponent {
  private readonly translations = provideTranslations({ en, ru });

  private readonly layoutService = inject(LayoutService);
  private readonly locale = inject(LOCALE_ID);
  protected readonly l10nService = inject(L10nService);
  protected readonly userService = inject(UserService);
  protected readonly securityService = inject(SecurityService);
  protected readonly menuService = inject(MenuService);
  protected readonly dateFormats: string[] = ['dd.MM.yyyy', 'dd.MM.yy', 'yyyy-MM-dd', 'dd.MMM.yyyy'];
  protected readonly timeFormats: string[] = ['HH:mm:ss', 'HH:mm'];
  // The workspace compiles against lib es2018, where Intl.supportedValuesOf isn't declared yet.
  protected readonly allTimeZones: string[] = (
    Intl as unknown as { supportedValuesOf(key: 'timeZone'): string[] }
  ).supportedValuesOf('timeZone');

  protected readonly adminLinks = [
    { id: 'oip-app-config-modules-link', path: '/modules', icon: 'pi pi-cog', label: 'config.modules' },
    {
      id: 'oip-app-config-applications-link',
      path: '/applications',
      icon: 'pi pi-th-large',
      label: 'config.applications'
    },
    { id: 'oip-app-config-sessions-link', path: '/sessions', icon: 'pi pi-users', label: 'config.sessions' }
  ];

  protected selectedLanguage: string;
  protected selectedDateFormat: string;
  protected selectedTimeFormat: string;
  protected selectedTimeZone: string;
  protected selectedFirstDayOfWeek: number;

  private readonly currentUser = toSignal(this.securityService.getCurrentUser$());
  protected readonly email = computed(() => this.currentUser()?.email as string | undefined);
  protected readonly roles = computed(() =>
    ((this.currentUser()?.roles as string[] | undefined) ?? []).filter(
      (role) => !HIDDEN_ROLES.includes(role) && !role.startsWith(HIDDEN_ROLE_PREFIX)
    )
  );

  private readonly now = signal(new Date());

  /** Current date and time in the chosen formats and time zone. */
  protected readonly nowPreview = computed(() => {
    const config = this.layoutService.layoutConfig();
    return formatDate(
      this.now(),
      `${config.dateFormat} ${config.timeFormat}`,
      this.locale,
      this.getTimeZoneOffset(config.timeZone)
    );
  });

  /** Days of the week starting from Monday, named in the current language. */
  protected readonly weekDays = computed(() => {
    const format = new Intl.DateTimeFormat(this.layoutService.language() ?? 'en', { weekday: 'long' });
    // 1 January 2024 is a Monday, so day N of that week falls on N January (Sunday - on 7 January).
    return [1, 2, 3, 4, 5, 6, 0].map((day) => {
      const name = format.format(new Date(2024, 0, day || 7));
      return { value: day, label: name.charAt(0).toUpperCase() + name.slice(1) };
    });
  });

  constructor() {
    this.selectedLanguage = this.layoutService.language();
    this.selectedDateFormat = this.layoutService.dateFormat();
    this.selectedTimeFormat = this.layoutService.timeFormat();
    this.selectedTimeZone = this.layoutService.timeZone();
    this.selectedFirstDayOfWeek = this.layoutService.firstDayOfWeek();

    const timer = setInterval(() => this.now.set(new Date()), PREVIEW_REFRESH_MS);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  changeLanguage() {
    this.layoutService.layoutConfig.update((config) => ({
      ...config,
      language: this.selectedLanguage
    }));
    this.l10nService.use(this.selectedLanguage, 'config');
  }

  changeDateFormat() {
    this.layoutService.layoutConfig.update((config) => ({ ...config, dateFormat: this.selectedDateFormat }));
  }

  changeTimeFormat() {
    this.layoutService.layoutConfig.update((config) => ({ ...config, timeFormat: this.selectedTimeFormat }));
  }

  changeFirstDayOfWeek() {
    this.layoutService.layoutConfig.update((config) => ({ ...config, firstDayOfWeek: this.selectedFirstDayOfWeek }));
  }

  changeTimeZone() {
    this.layoutService.layoutConfig.update((config) => ({ ...config, timeZone: this.selectedTimeZone }));
  }

  async onSwitchChange(): Promise<void> {
    await this.menuService.loadMenu();
  }

  /**
   * Offset of the time zone in the form the date pipe takes ("+0300"), or undefined for the browser time zone.
   */
  private getTimeZoneOffset(timeZone: string | undefined): string | undefined {
    if (!timeZone) {
      return undefined;
    }

    try {
      const name = new Intl.DateTimeFormat('en-US', { timeZone, timeZoneName: 'longOffset' })
        .formatToParts(this.now())
        .find((part) => part.type === 'timeZoneName')?.value;
      // "GMT+03:00" for most zones, plain "GMT" for UTC itself.
      return name === 'GMT' ? '+0000' : name?.replace('GMT', '').replace(':', '');
    } catch {
      return undefined;
    }
  }
}
