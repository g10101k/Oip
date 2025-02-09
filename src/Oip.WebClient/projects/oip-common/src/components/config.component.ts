import { Component, inject } from '@angular/core';
import { ProfileComponent } from "./profile.component";
import { Fluid } from "primeng/fluid";
import { Tooltip } from "primeng/tooltip";
import { FormsModule } from "@angular/forms";
import { Select } from "primeng/select";
import { TableModule } from "primeng/table";
import { LayoutService } from "../services/app.layout.service";
import { TranslatePipe } from "@ngx-translate/core";

@Component({
  selector: 'app-config',
  template: `
    <p-fluid>
      <div class="flex flex-col md:flex-row gap-4">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl"> {{ 'configComponent.profile' | translate }}</div>
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
      </div>
    </p-fluid>
  `,
  imports: [ProfileComponent, Fluid, Tooltip, FormsModule, Select, TableModule, TranslatePipe]
})
export class ConfigComponent {
  private readonly layoutService = inject(LayoutService);
  selectedLanguage: any;
  languages: any[] = [
    { value: "en", label: "English" },
    { value: "ru", label: "Русский" },
  ];

  constructor() {
    this.selectedLanguage = this.layoutService.language();
  }

  changeLanguage($event) {
    this.layoutService.layoutConfig.update((config) => ({ ...config, language: this.selectedLanguage }));
  }
}
