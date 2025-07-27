import {Component, inject} from '@angular/core';
import {ProfileComponent} from "./profile.component";
import {Fluid} from "primeng/fluid";
import {Tooltip} from "primeng/tooltip";
import {FormsModule} from "@angular/forms";
import {Select} from "primeng/select";
import {TableModule} from "primeng/table";
import {LayoutService} from "../services/app.layout.service";
import {TranslatePipe} from "@ngx-translate/core";
import {UserService} from "../services/user.service";
import {InputSwitch} from "primeng/inputswitch";
import {NgIf} from "@angular/common";
import {RouterLink} from "@angular/router";
import {SecurityService} from "../services/security.service";
import {MenuService} from "../services/app.menu.service";

@Component({
  selector: 'app-config',
  template: `
    <p-fluid>
      <div class="flex flex-col md:flex-row gap-4">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl"> {{ 'configComponent.profile' | translate }}</div>
            <div class="flex justify-content-end flex-wrap">{{ userService.userName }}</div>
            <label> {{ 'configComponent.photo'  | translate }} <span
              pTooltip="{{ 'configComponent.usePhoto256x256Pixel' | translate }}"
              tooltipPosition="right"
              class="pi pi-question-circle"></span></label>
            <div class="flex justify-content-end flex-wrap">
              <user-profile></user-profile>
            </div>
          </div>
        </div>
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl"> {{ 'configComponent.localization' | translate }}</div>
            <label> {{ 'configComponent.selectLanguage' | translate }} </label>
            <div class="flex justify-content-end flex-wrap">
              <p-select [options]="languages"
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
            <div class="font-semibold text-xl">Меню</div>
            <div class="flex items-center gap-2">
              <label for="adminMode">{{ 'menuComponent.all' | translate }}</label>
              <p-inputSwitch id="adminMode" [(ngModel)]="menuService.adminMode"
                             (onChange)="onSettingButtonClick()"></p-inputSwitch>
            </div>
            <a routerLink="/modules"> <i class="pi pi-cog"></i></a>
          </div>
        </div>
      </div>
    </p-fluid>
  `,
  imports: [ProfileComponent, Fluid, Tooltip, FormsModule, Select, TableModule, TranslatePipe, InputSwitch, NgIf, RouterLink]
})
export class ConfigComponent {
  private readonly layoutService = inject(LayoutService);
  protected readonly userService = inject(UserService);
  protected readonly securityService = inject(SecurityService);
  protected readonly menuService = inject(MenuService);

  selectedLanguage: any;
  languages: any[] = [
    {value: "en", label: "English"},
    {value: "ru", label: "Русский"},
  ];

  constructor() {
    this.selectedLanguage = this.layoutService.language();
  }

  /**
   * Changes the application's language.
   * @param {Event} $event The event triggered by the language selection.
   * @return {void}
   */
  changeLanguage($event) {
    this.layoutService.layoutConfig.update((config) => ({...config, language: this.selectedLanguage}));
  }

  /**
   * Handles the button click event for settings.
   * Asynchronously loads the menu using the menu service.
   * @return {Promise<void>} A Promise that resolves when the menu loading is complete.
   */
  async onSettingButtonClick(): Promise<void> {
    await this.menuService.loadMenu();
  }
}
