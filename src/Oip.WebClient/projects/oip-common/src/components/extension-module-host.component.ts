import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { BaseModuleComponent } from './base-module.component';
import { SecurityComponent } from './security.component';
import { ExtensionLoaderService } from '../extension-host/extension-loader.service';
import { OIP_EXTENSION_EVENTS, emitOipContextChange } from '../extension-host/extension-host.events';
import {
  OipExtensionHostContext,
  OipExtensionModuleMetadata,
  OipExtensionNavigateEvent,
  OipExtensionNotifyEvent
} from '../extension-host/extension-host.types';

@Component({
  standalone: true,
  imports: [CommonModule, SecurityComponent, TranslatePipe, Button],
  template: `
    @if (isContent) {
      <div class="min-h-[calc(100vh-10rem)] w-full">
        @if (loadError) {
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ loadError }}</div>
            <p-button icon="pi pi-refresh" label="Retry" (onClick)="reloadExtension()"></p-button>
          </div>
        } @else {
          <div #extensionContainer class="w-full"></div>
        }
      </div>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'baseComponent.settings' | translate }}</div>
            <p-button icon="pi pi-save" label="Save" (onClick)="saveSettings(settings)"></p-button>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `
})
export class ExtensionModuleHostComponent extends BaseModuleComponent<unknown, unknown>
  implements OnDestroy {
  @ViewChild('extensionContainer')
  private set extensionContainer(element: ElementRef<HTMLElement> | undefined) {
    this.container = element;
    void this.renderExtension();
  }

  private readonly extensionLoader = inject(ExtensionLoaderService);
  private readonly router = inject(Router);
  private container?: ElementRef<HTMLElement>;
  private extensionElement?: HTMLElement;
  private extensionMetadata?: OipExtensionModuleMetadata;
  private removeListeners: Array<() => void> = [];

  protected loadError: string | null = null;

  private get extensionKey(): string {
    return this.route.snapshot.paramMap.get('extensionKey') ?? '';
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    await this.loadExtensionMetadata();
    await this.renderExtension();
  }

  protected override onSecurityRightsChange(): void {
    this.updateExtensionContext();
  }

  override ngOnDestroy(): void {
    this.destroyExtensionElement();
    super.ngOnDestroy();
  }

  async reloadExtension(): Promise<void> {
    this.loadError = null;
    await this.loadExtensionMetadata();
    await this.renderExtension();
  }

  private async loadExtensionMetadata(): Promise<void> {
    if (!this.extensionKey) {
      this.loadError = 'Extension key is missing.';
      return;
    }

    this.extensionMetadata = await this.baseDataService.sendRequest<OipExtensionModuleMetadata>(
      `api/extension-modules/get-extension-module-by-key/${this.extensionKey}`
    );
  }

  private async renderExtension(): Promise<void> {
    if (!this.container || !this.extensionMetadata || !this.isContent) {
      return;
    }

    const {elementName, scriptUrl} = this.extensionMetadata;
    if (!elementName || !scriptUrl) {
      this.loadError = 'Extension metadata is incomplete.';
      return;
    }

    try {
      this.loadError = null;
      await this.extensionLoader.loadScript(scriptUrl);
      await customElements.whenDefined(elementName);
      this.destroyExtensionElement();

      const element = document.createElement(elementName);
      this.extensionElement = element;
      this.attachListeners(element);
      this.container.nativeElement.appendChild(element);
      this.updateExtensionContext();
    } catch (error) {
      this.loadError = error instanceof Error ? error.message : 'Extension could not be loaded.';
      this.msgService.error(error);
    }
  }

  private updateExtensionContext(): void {
    if (!this.extensionElement || !this.extensionMetadata?.extensionKey) {
      return;
    }

    const context: OipExtensionHostContext = {
      moduleInstanceId: this.id,
      extensionKey: this.extensionMetadata.extensionKey,
      apiBasePath: this.baseDataService.buildUrl(`api/extensions/${this.extensionMetadata.extensionKey}`),
      settings: this.settings,
      locale: this.layoutService.language(),
      theme: this.layoutService.layoutConfig(),
      user: this.securityService.getCurrentUser(),
      permissions: {
        canRead: this.canRead,
        canEdit: this.canEdit,
        canDelete: this.canDelete
      }
    };

    Object.assign(this.extensionElement, {oipContext: context});
    emitOipContextChange(this.extensionElement, context);
  }

  private attachListeners(element: HTMLElement): void {
    this.addListener(element, OIP_EXTENSION_EVENTS.titleChange, (event) => {
      const title = (event as CustomEvent<string>).detail;
      if (title) {
        this.appTitleService.setTitle(title);
      }
    });

    this.addListener(element, OIP_EXTENSION_EVENTS.settingsChange, (event) => {
      this.settings = (event as CustomEvent<unknown>).detail;
      void this.saveSettings(this.settings);
    });

    this.addListener(element, OIP_EXTENSION_EVENTS.notify, (event) => {
      const detail = (event as CustomEvent<OipExtensionNotifyEvent>).detail;
      const severity = detail?.severity ?? 'info';
      const message = detail?.detail ?? detail?.summary ?? '';
      switch (severity) {
        case 'success':
          this.msgService.success(message, detail?.summary);
          break;
        case 'warn':
          this.msgService.warn(message, detail?.summary);
          break;
        case 'error':
          this.msgService.error(message, detail?.summary);
          break;
        default:
          this.msgService.info(message, detail?.summary);
      }
    });

    this.addListener(element, OIP_EXTENSION_EVENTS.navigate, (event) => {
      const detail = (event as CustomEvent<OipExtensionNavigateEvent>).detail;
      if (detail?.commands) {
        void this.router.navigate(detail.commands as any[]);
      } else if (detail?.url) {
        void this.router.navigateByUrl(detail.url);
      }
    });

    this.addListener(element, OIP_EXTENSION_EVENTS.error, (event) => {
      this.msgService.error((event as CustomEvent<unknown>).detail);
    });
  }

  private addListener(element: HTMLElement, eventName: string, listener: EventListener): void {
    element.addEventListener(eventName, listener);
    this.removeListeners.push(() => element.removeEventListener(eventName, listener));
  }

  private destroyExtensionElement(): void {
    this.removeListeners.forEach((remove) => remove());
    this.removeListeners = [];
    this.extensionElement?.remove();
    this.extensionElement = undefined;
  }
}
