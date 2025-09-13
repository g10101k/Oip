import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ProfileComponent } from "./profile.component";
import { Fluid } from "primeng/fluid";
import { Tooltip } from "primeng/tooltip";
import { FormsModule } from "@angular/forms";
import { Select, SelectChangeEvent } from "primeng/select";
import { TableModule } from "primeng/table";
import { LayoutService } from "../services/app.layout.service";
import { UserService } from "../services/user.service";
import { InputSwitch } from "primeng/inputswitch";
import { NgIf } from "@angular/common";
import { RouterLink } from "@angular/router";
import { SecurityService } from "../services/security.service";
import { MenuService } from "../services/app.menu.service";
import { Button } from "primeng/button";
import { L10nService } from "../services/l10n.service";

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
            <div class="font-semibold text-xl"> {{ l10n.profile }}</div>
            <div class="flex justify-content-end flex-wrap">{{ userService.userName }}</div>
            <label> {{ l10n.photo }}
              <span
                pTooltip="{{ l10n.usePhoto256x256Pixel }}"
                tooltipPosition="right"
                class="pi pi-question-circle"></span>
            </label>
            <div class="flex justify-content-end flex-wrap">
              <user-profile></user-profile>
            </div>
          </div>
        </div>
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl"> {{ l10n.localization }}</div>
            <label> {{ l10n.selectLanguage }} </label>
            <div class="flex justify-content-end flex-wrap">
              <p-select id="oip-app-config-language-select"
                        [options]="languages"
                        [(ngModel)]="selectedLanguage"
                        (onChange)="changeLanguage($event)"
                        optionLabel="label"
                        optionValue="value"
                        class="w-full md:w-56"/>
            </div>
          </div>
        </div>
        <div *ngIf="securityService.isAdmin" class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ l10n.menu }}</div>
            <div class="flex items-center gap-2">
              <label for="oip-app-config-admin-mode">{{ l10n.all }}</label>
              <p-inputSwitch id="oip-app-config-admin-mode" [(ngModel)]="menuService.adminMode"
                             (onChange)="onSwitchChange()"></p-inputSwitch>
            </div>
            <div class="flex items-center gap-2">
              <label for="oip-app-config-admin-mode">{{ l10n.moduleManagement }}</label>
              <p-button routerLink="/modules" icon="pi pi-cog" label="{{ l10n.goTo }}"/>

            </div>
          </div>
        </div>
      </div>
    </p-fluid>
  `,
  imports: [ProfileComponent, Fluid, Tooltip, FormsModule, Select, TableModule, InputSwitch, NgIf, RouterLink, Button]
})
export class ConfigComponent implements OnInit {
  private readonly layoutService = inject(LayoutService);
  private readonly l10nService = inject(L10nService);
  protected readonly userService = inject(UserService);
  protected readonly securityService = inject(SecurityService);
  protected readonly menuService = inject(MenuService);
  protected l10n = {} as L10n;

  selectedLanguage: any;
  languages: any[] = [
    { value: "en", label: "English" },
    { value: "ru", label: "Русский" },
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
   * @param {Event} $event The event triggered by the language selection.
   * @return {void}
   */
  changeLanguage($event: SelectChangeEvent): void {
    this.layoutService.layoutConfig.update((config) => ({ ...config, language: this.selectedLanguage }));
    this.l10nService.use(this.selectedLanguage);
  }

  async onSwitchChange(): Promise<void> {
    await this.menuService.loadMenu();
  }
}
