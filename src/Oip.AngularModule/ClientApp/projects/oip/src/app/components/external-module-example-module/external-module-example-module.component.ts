import { Component } from '@angular/core';
import { BaseModuleComponent, NoSettingsDto, SecurityComponent } from 'oip-common';
import { ExternalModuleExampleModuleSettings } from '../../../api/data-contracts';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-external-module-example-module',
  imports: [SecurityComponent, TranslatePipe, Button, InputText, ReactiveFormsModule, FormsModule],
  template: `
    @if (isContent) {
      <div class="card space-y-4">
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h5 class="mb-1">{{ title }}</h5>
            <p class="m-0 text-surface-500">{{ 'external-module-example-module.content.subtitle' | translate }}</p>
          </div>
        </div>
      </div>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'external-module-example-module.settings.title' | translate }}</div>
            <div class="grid grid-cols-12 gap-4">
              <label class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0" for="dayCount">
                {{ 'external-module-example-module.settings.dayCount' | translate }}
              </label>
              <div class="col-span-12 md:col-span-10">
                <input id="dayCount" pInputText type="text" [(ngModel)]="settings"/>
              </div>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                [label]="'external-module-example-module.settings.save' | translate"
                (onClick)="saveSettings(settings)"></p-button>
            </div>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `
})
export class ExternalModuleExampleModuleComponent extends BaseModuleComponent<ExternalModuleExampleModuleSettings, NoSettingsDto> {
  protected override async onModuleInstanceChange(): Promise<void> {
    // Initialize module data here.
  }
}