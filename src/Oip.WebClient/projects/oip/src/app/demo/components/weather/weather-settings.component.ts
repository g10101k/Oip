import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { Fluid } from "primeng/fluid";

export interface WeatherSettingsDto {
  dayCount: number;
}

@Component({
  selector: 'weather-settings',
  template: `
    <p-fluid>
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">Weather settings</div>
            <div class="grid grid-cols-12 gap-4">
              <label for="dayCount" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Day Count</label>
              <div class="col-span-12 md:col-span-10">
                <input pInputText [(ngModel)]="settings.dayCount" id="dayCount" type="text"/>
              </div>
            </div>
            <div class="flex justify-end">
              <p-button label="Save" icon="pi pi-save" (onClick)="saveButtonClick($event)"></p-button>
            </div>
          </div>
        </div>
      </div>
    </p-fluid>
  `,
  standalone: true,
  imports: [
    FormsModule,
    InputTextModule,
    ButtonModule,
    Fluid,
  ],
})
export class WeatherSettingsComponent {
  @Input() settings: WeatherSettingsDto;
  @Output() settingsChange = new EventEmitter<WeatherSettingsDto>();

  saveButtonClick($event: MouseEvent) {
    this.settingsChange.emit(this.settings);
  }
}
