import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { Fluid } from "primeng/fluid";

export interface WeatherSettingsDto {
  dayCount: number;
}

@Component({
  selector: 'tag-management-settings',
  template: `
    <div class="flex flex-col md:flex-row gap-8">
      <div class="md:w-1/2">
        <div class="card flex flex-col gap-4">
          <div class="font-semibold text-xl">Settings</div>
        </div>
      </div>
    </div>
  `,
  standalone: true,
  imports: [
    FormsModule,
    InputTextModule,
    ButtonModule,
    Fluid,
  ],
})
export class TagManagementSettings {
  @Input() settings: WeatherSettingsDto;
  @Output() settingsChange = new EventEmitter<WeatherSettingsDto>();

  saveButtonClick($event: MouseEvent) {
    this.settingsChange.emit(this.settings);
  }
}
