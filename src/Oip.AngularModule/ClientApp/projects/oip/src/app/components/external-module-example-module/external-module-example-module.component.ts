import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { BaseModuleComponent, NoSettingsDto, RequestParams, SecurityComponent } from 'oip-common';
import { ExternalModuleExampleDataDto, ExternalModuleExampleModuleSettings } from '../../../api/data-contracts';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ExternalModuleExampleModuleApi } from '../../../api/external-module-example-module.api';

@Component({
  selector: 'app-external-module-example-module',
  imports: [SecurityComponent, TranslatePipe, Button, InputText, ReactiveFormsModule, FormsModule, DatePipe],
  template: `
    @if (isContent) {
      <div class="card space-y-4">
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h5 class="mb-1">{{ title }}</h5>
            <p class="m-0 text-surface-500">{{ 'external-module-example-module.content.subtitle' | translate }}</p>
          </div>
        </div>

        @if (loading) {
          <div class="text-surface-500">{{ 'external-module-example-module.content.loading' | translate }}</div>
        } @else if (data) {
          <div class="grid grid-cols-12 gap-4">
            <div class="col-span-12 md:col-span-4">
              <div class="font-medium text-surface-500">
                {{ 'external-module-example-module.content.message' | translate }}
              </div>
              <div>{{ data.message }}</div>
            </div>
            <div class="col-span-12 md:col-span-4">
              <div class="font-medium text-surface-500">
                {{ 'external-module-example-module.content.generatedAt' | translate }}
              </div>
              <div>{{ data.generatedAt | date: layoutService.dateTimeFormat() }}</div>
            </div>
          </div>

          <ul class="m-0 pl-5">
            @for (item of data.items; track item) {
              <li>{{ item }}</li>
            }
          </ul>
        }
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
  `,
  providers: [ExternalModuleExampleModuleApi]
})
export class ExternalModuleExampleModuleComponent extends BaseModuleComponent<ExternalModuleExampleModuleSettings, NoSettingsDto> {
  protected readonly dataService = inject(ExternalModuleExampleModuleApi);
  protected readonly moduleApiRequestParams: RequestParams = {
    baseUrl: this.resolveModuleBackendOrigin(),
    credentials: 'include'
  };
  protected data?: ExternalModuleExampleDataDto;
  protected loading = false;

  protected override async onModuleInstanceChange(): Promise<void> {
    this.loading = true;

    try {
      this.data = await this.dataService.getExternalModuleExampleData(this.moduleApiRequestParams);
    } catch (error) {
      this.data = undefined;
      console.error(this.t('external-module-example-module.errorFetchingMessage'), error);
      this.msgService.errorFromException(error, this.t('external-module-example-module.errorFetchingMessage'));
    } finally {
      this.loading = false;
    }
  }

  private resolveModuleBackendOrigin(): string {
    const remoteEntryUrl = this.findRemoteEntryUrl();

    if (!remoteEntryUrl) {
      return '';
    }

    try {
      return new URL(remoteEntryUrl).origin;
    } catch {
      return '';
    }
  }

  private findRemoteEntryUrl(): string | undefined {
    const scriptUrls = Array.from(document.scripts)
      .map((script) => script.src)
      .filter(Boolean);
    const resourceUrls = performance
      .getEntriesByType('resource')
      .map((entry) => entry.name)
      .filter(Boolean);

    return [...scriptUrls, ...resourceUrls]
      .reverse()
      .find((url) => url.endsWith('/remoteEntry.js') && new URL(url).origin !== window.location.origin)
      ?? [...scriptUrls, ...resourceUrls].reverse().find((url) => url.endsWith('/remoteEntry.js'));
  }
}
