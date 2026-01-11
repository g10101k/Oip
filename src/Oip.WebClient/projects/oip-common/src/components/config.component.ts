import { Component, inject } from '@angular/core';
import { ProfileComponent } from './profile.component';
import { Tooltip } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { LayoutService } from '../services/app.layout.service';
import { UserService } from '../services/user.service';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { RouterLink } from '@angular/router';
import { SecurityService } from '../services/security.service';
import { MenuService } from '../services/app.menu.service';
import { Button } from 'primeng/button';
import { L10nService } from '../services/l10n.service';
import { TranslatePipe } from '@ngx-translate/core';

interface L10n {
  menu: string;
  all: string;
  profile: string;
  photo: string;
  usePhoto256x256Pixel: string;
  selectLanguage: string;
  moduleManagement: string;
  localization: string;
  goTo: string;
}

@Component({
  selector: 'app-config',
  template: `
    <div class="flex flex-col md:flex-row gap-4">
      <div class="md:w-1/2">
        <div class="card flex flex-col gap-4">
          <div class="font-semibold text-xl">{{ 'config.profile' | translate }}</div>
          <div class="flex justify-content-end flex-wrap">
            {{ userService.userName }}
          </div>
          <label>
            {{ 'config.photo' | translate }}
            <span
              class="pi pi-question-circle"
              pTooltip="{{ 'config.usePhoto256x256Pixel' | translate }}"
              tooltipPosition="right"></span>
          </label>
          <div class="flex justify-content-end flex-wrap">
            <user-profile></user-profile>
          </div>
        </div>
      </div>
      <div class="md:w-1/2">
        <div class="card flex flex-col gap-4">
          <div class="font-semibold text-xl">{{ 'config.localization' | translate }}</div>
          <label> {{ 'config.selectLanguage' | translate }} </label>
          <div class="flex justify-content-end flex-wrap">
            <p-select
              class="w-full md:w-56"
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
          <div class="flex flex-col gap-5">
            <div class="mt-5">{{ 'config.dateTimeFormat' | translate }}</div>
            <p-select
              class="w-full md:w-56"
              qa-id="oip-app-config-date-format-select"
              [options]="dateFormats"
              [placeholder]="'config.dateFormat' | translate"
              [(ngModel)]="selectedDateFormat"
              (onChange)="changeDateFormat()" />
            <p-select
              class="w-full md:w-56"
              qa-id="oip-app-config-time-format-select"
              [options]="timeFormats"
              [placeholder]="'config.timeFormat' | translate"
              [(ngModel)]="selectedTimeFormat"
              (onChange)="changeTimeFormat()" />
            <div class="mt-5">{{ 'config.timeZone' | translate }}</div>
            <p-select
              class="w-full md:w-56"
              qa-id="oip-app-config-timezone-select"
              [filter]="true"
              [options]="allTimeZones"
              [placeholder]="'config.timeZone' | translate"
              [virtualScroll]="true"
              [virtualScrollItemSize]="34"
              [(ngModel)]="selectedTimeZone"
              (onChange)="changeTimeZone()" />
          </div>
        </div>
      </div>
      @if (securityService.isAdmin()) {
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'config.menu' | translate }}</div>
            <div class="flex items-center gap-2">
              <label for="oip-app-config-admin-mode">{{ 'config.all' | translate }}</label>
              <p-toggle-switch
                id="oip-app-config-admin-mode"
                [(ngModel)]="menuService.adminMode"
                (onChange)="onSwitchChange()"></p-toggle-switch>
            </div>
            <div class="flex items-center gap-2">
              <label for="oip-app-config-admin-mode">{{ 'config.moduleManagement' | translate }}</label>
              <p-button icon="pi pi-cog" label="{{ 'config.goTo' | translate }}" routerLink="/modules" />
            </div>
          </div>
        </div>
      }
    </div>
  `,
  imports: [
    ProfileComponent,
    Tooltip,
    FormsModule,
    Select,
    TableModule,
    TranslatePipe,
    ToggleSwitchModule,
    Button,
    RouterLink
  ]
})
export class ConfigComponent {
  private readonly layoutService = inject(LayoutService);
  protected readonly l10nService = inject(L10nService);
  protected readonly userService = inject(UserService);
  protected readonly securityService = inject(SecurityService);
  protected readonly menuService = inject(MenuService);
  protected l10n = {} as L10n;
  protected readonly dateFormats: string[] = ['dd.MM.yyyy', 'dd.MM.yy', 'yyyy-MM-dd', 'dd.MMM.yyyy'];
  protected readonly timeFormats: string[] = ['HH:mm:ss', 'HH:mm'];
  // @ts-ignore
  protected readonly allTimeZones: any[] = Intl.supportedValuesOf('timeZone');

  protected selectedLanguage: any;
  protected selectedDateFormat: string;
  protected selectedTimeFormat: string;
  protected selectedTimeZone: string;

  constructor() {
    this.l10nService.loadComponentTranslations('config');
    this.selectedLanguage = this.layoutService.language();
    this.selectedDateFormat = this.layoutService.dateFormat();
    this.selectedTimeFormat = this.layoutService.timeFormat();
    this.selectedTimeZone = this.layoutService.timeZone();
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

  changeTimeZone() {
    this.layoutService.layoutConfig.update((config) => ({ ...config, timeZone: this.selectedTimeZone }));
  }

  async onSwitchChange(): Promise<void> {
    await this.menuService.loadMenu();
  }
}
