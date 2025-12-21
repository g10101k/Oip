import { Component, inject, OnInit } from '@angular/core';
import { ProfileComponent } from './profile.component';
import { Fluid } from 'primeng/fluid';
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
import { SelectItem } from 'primeng/api';
import { TranslatePipe } from "@ngx-translate/core";

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
    <p-fluid>
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
                id="oip-app-config-language-select"
                optionLabel="label"
                optionValue="value"
                [options]="languages"
                [(ngModel)]="selectedLanguage"
                (onChange)="changeLanguage()"/>
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
                <p-button icon="pi pi-cog" label="{{ 'config.goTo' | translate  }}" routerLink="/modules"/>
              </div>
            </div>
          </div>
        }
      </div>
    </p-fluid>
  `,
  imports: [ProfileComponent, Fluid, Tooltip, FormsModule, Select, TableModule, ToggleSwitchModule, RouterLink, Button, TranslatePipe]
})
export class ConfigComponent implements OnInit {
  private readonly layoutService = inject(LayoutService);
  private readonly l10nService = inject(L10nService);
  protected readonly userService = inject(UserService);
  protected readonly securityService = inject(SecurityService);
  protected readonly menuService = inject(MenuService);
  protected l10n = {} as L10n;

  selectedLanguage: string;
  languages: SelectItem<string>[] = [
    { value: 'en', label: 'English' },
    { value: 'ru', label: 'Русский' }
  ];

  constructor() {
    this.l10nService.loadComponentTranslations('config');
    this.selectedLanguage = this.layoutService.language();
  }

  async ngOnInit() {

  }

  /**
   * Changes the application's language.
   * @return {void}
   */
  changeLanguage(): void {
    this.layoutService.layoutConfig.update((config) => ({
      ...config,
      language: this.selectedLanguage
    }));
    this.l10nService.use(this.selectedLanguage, 'config');
  }

  async onSwitchChange(): Promise<void> {
    await this.menuService.loadMenu();
  }
}
