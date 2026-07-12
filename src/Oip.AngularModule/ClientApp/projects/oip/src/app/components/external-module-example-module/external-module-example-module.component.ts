import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, inject } from '@angular/core';
import {
  BaseModuleComponent,
  NoSettingsDto,
  RequestParams,
  SecurityComponent
} from 'oip-common';
import { ExternalModuleExampleDataDto, ExternalModuleExampleModuleSettings } from '../../../api/data-contracts';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ExternalModuleExampleModuleApi } from '../../../api/external-module-example-module.api';

interface OipExtensionHostContext {
  activeTabId?: string;
  settings?: unknown;
}

type ExtensionHostElement = HTMLElement & {
  oipContext?: OipExtensionHostContext;
};

const contextChangeEventName = 'oip:context-change';

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
                <input id="dayCount" pInputText type="number" min="1" [(ngModel)]="settings.dayCount"/>
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
  private readonly hostElement = inject(ElementRef<ExtensionHostElement>);
  private readonly moduleChangeDetectorRef = inject(ChangeDetectorRef);
  protected readonly moduleApiRequestParams: RequestParams = {
    baseUrl: this.resolveModuleBackendOrigin(),
    credentials: 'include'
  };
  protected data?: ExternalModuleExampleDataDto;
  protected loading = false;
  private hostActiveTabId?: string;

  override get isContent(): boolean {
    return this.isHostTabActive('content') ?? super.isContent;
  }

  override get isSettings(): boolean {
    return this.isHostTabActive('settings') ?? super.isSettings;
  }

  override get isSecurity(): boolean {
    return this.isHostTabActive('security') ?? super.isSecurity;
  }

  override async ngOnInit(): Promise<void> {
    this.applyDefaultSettings();
    this.applyHostContext(this.hostElement.nativeElement.oipContext);
    this.hostElement.nativeElement.addEventListener(contextChangeEventName, this.onHostContextChange);

    await super.ngOnInit();
    this.applyDefaultSettings();
    this.applyHostContext(this.hostElement.nativeElement.oipContext);
  }

  override ngOnDestroy(): void {
    this.hostElement.nativeElement.removeEventListener(contextChangeEventName, this.onHostContextChange);
    super.ngOnDestroy();
  }

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

  private readonly onHostContextChange = (event: Event): void => {
    this.applyHostContext((event as CustomEvent<OipExtensionHostContext>).detail);
    this.moduleChangeDetectorRef.detectChanges();
  };

  private applyHostContext(context: OipExtensionHostContext | undefined): void {
    if (!context) {
      return;
    }

    this.hostActiveTabId = context.activeTabId;
    this.settings = {
      ...this.settings,
      ...(context.settings as ExternalModuleExampleModuleSettings | undefined)
    };
    this.applyDefaultSettings();
  }

  private applyDefaultSettings(): void {
    this.settings = {
      dayCount: 7,
      ...this.settings
    };
  }

  private isHostTabActive(tabId: string): boolean | undefined {
    return this.hostActiveTabId ? this.hostActiveTabId === tabId : undefined;
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
