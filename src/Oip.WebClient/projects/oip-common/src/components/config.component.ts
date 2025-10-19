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
            <div class="font-semibold text-xl">{{ l10n.profile }}</div>
            <div class="flex justify-content-end flex-wrap">
              {{ userService.userName }}
            </div>
            <label>
              {{ l10n.photo }}
              <span
                class="pi pi-question-circle"
                pTooltip="{{ l10n.usePhoto256x256Pixel }}"
                tooltipPosition="right"></span>
            </label>
            <div class="flex justify-content-end flex-wrap">
              <user-profile></user-profile>
            </div>
          </div>
        </div>
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ l10n.localization }}</div>
            <label> {{ l10n.selectLanguage }} </label>
            <div class="flex justify-content-end flex-wrap">
              <p-select
                class="w-full md:w-56"
                id="oip-app-config-language-select"
                optionLabel="label"
                optionValue="value"
                [options]="languages"
                [(ngModel)]="selectedLanguage"
                (onChange)="changeLanguage()" />
            </div>
          </div>
        </div>
        @if (securityService.isAdmin) {
          <div class="md:w-1/2">
            <div class="card flex flex-col gap-4">
              <div class="font-semibold text-xl">{{ l10n.menu }}</div>
              <div class="flex items-center gap-2">
                <label for="oip-app-config-admin-mode">{{ l10n.all }}</label>
                <p-toggle-switch
                  id="oip-app-config-admin-mode"
                  [(ngModel)]="menuService.adminMode"
                  (onChange)="onSwitchChange()"></p-toggle-switch>
              </div>
              <div class="flex items-center gap-2">
                <label for="oip-app-config-admin-mode">{{ l10n.moduleManagement }}</label>
                <p-button icon="pi pi-cog" label="{{ l10n.goTo }}" routerLink="/modules" />
              </div>
            </div>
          </div>
        }
      </div>
    </p-fluid>
  `,
  imports: [ProfileComponent, Fluid, Tooltip, FormsModule, Select, TableModule, ToggleSwitchModule, RouterLink, Button]
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
    this.selectedLanguage = this.layoutService.language();
  }

  async ngOnInit() {
    (await this.l10nService.get('config')).subscribe((l10n) => {
      this.l10n = l10n;
    });
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
    this.l10nService.use(this.selectedLanguage);
  }

  async onSwitchChange(): Promise<void> {
    await this.menuService.loadMenu();
  }
}
