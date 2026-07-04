import { ChangeDetectorRef, Component, ElementRef, inject, OnDestroy, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { SecurityComponent } from "./security.component";
import { TranslatePipe } from "@ngx-translate/core";
import { Button } from "primeng/button";
import { firstValueFrom } from "rxjs";
import { ExtensionModulesApi } from "../api/extension-modules.api";
import { BaseModuleComponent } from "./base-module.component";
import { ExtensionLoaderService } from "../extension-host/extension-loader.service";
import { Router } from "@angular/router";
import {
  OipExtensionHostContext,
  OipExtensionModuleMetadata,
  OipExtensionNavigateEvent,
  OipExtensionNotifyEvent
} from "../extension-host/extension-host.types";
import { emitOipContextChange, OIP_EXTENSION_EVENTS } from "../extension-host/extension-host.events";

@Component({
  standalone: true,
  imports: [CommonModule, SecurityComponent, TranslatePipe, Button],
  providers: [ExtensionModulesApi],
  template: `
    @if (showContent) {
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
    } @else if (showSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'baseComponent.settings' | translate }}</div>
            <p-button icon="pi pi-save" label="Save" (onClick)="saveSettings(settings)"></p-button>
          </div>
        </div>
      </div>
    } @else if (showSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `
})
export class CustomElementExtensionModuleHostComponent extends BaseModuleComponent<unknown, unknown>
  implements OnDestroy {
  @ViewChild('extensionContainer')
  private set extensionContainer(element: ElementRef<HTMLElement> | undefined) {
    this.container = element;
    this.queueRenderExtension();
  }

  private readonly extensionLoader = inject(ExtensionLoaderService);
  private readonly router = inject(Router);
  private readonly hostChangeDetectorRef = inject(ChangeDetectorRef);
  private readonly extensionModulesApi = inject(ExtensionModulesApi);
  private container?: ElementRef<HTMLElement>;
  private extensionElement?: HTMLElement;
  private extensionMetadata?: OipExtensionModuleMetadata;
  private removeListeners: Array<() => void> = [];
  private renderQueued = false;
  private activeTabSyncQueued = false;
  private activeTabId = this.getCurrentActiveTabId();
  private destroyed = false;

  protected loadError: string | null = null;

  protected get showContent(): boolean {
    return this.isActiveTab('content');
  }

  protected get showSettings(): boolean {
    return this.isActiveTab('settings');
  }

  protected get showSecurity(): boolean {
    return this.isActiveTab('security');
  }

  private get extensionKey(): string {
    return this.route.snapshot.paramMap.get('extensionKey') ?? '';
  }

  constructor() {
    super();
    this.subscriptions.push(
      this.topBarService.activeId$.subscribe(() => {
        this.queueActiveTabSync();
      })
    );
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    await this.loadExtensionMetadata();
    await this.renderExtension();
  }

  protected override onSecurityRightsChange(): void {
    this.updateExtensionContext();
  }

  override ngOnDestroy(): void {
    this.destroyed = true;
    this.destroyExtensionElement();
    super.ngOnDestroy();
  }

  async reloadExtension(): Promise<void> {
    this.loadError = null;
    this.extensionMetadata = undefined;
    await this.loadExtensionMetadata();
    await this.renderExtension();
  }

  private async loadExtensionMetadata(): Promise<void> {
    if (this.extensionMetadata) {
      return;
    }

    if (!this.extensionKey) {
      this.loadError = 'Extension key is missing.';
      return;
    }

    this.extensionMetadata = await this.extensionModulesApi.getExtensionModuleByKey({
      extensionKey: this.extensionKey
    }) as unknown as OipExtensionModuleMetadata;

    await this.loadExtensionTranslations(this.extensionMetadata);
  }

  private async renderExtension(): Promise<void> {
    if (this.destroyed || !this.container || !this.extensionMetadata || !this.showContent) {
      return;
    }

    try {
      this.loadError = null;
      this.destroyExtensionElement();
      await this.renderCustomElementExtension(this.extensionMetadata);
    } catch (error) {
      this.loadError = error instanceof Error ? error.message : 'Extension could not be loaded.';
      this.msgService.error(error);
    }
  }

  private queueRenderExtension(): void {
    if (this.renderQueued) {
      return;
    }
    this.renderQueued = true;
    queueMicrotask(() => {
      this.renderQueued = false;
      void this.renderExtension();
    });
  }

  private isActiveTab(id: string): boolean {
    return this.activeTabId === id;
  }

  private queueActiveTabSync(): void {
    const nextActiveTabId = this.getCurrentActiveTabId();
    if (this.activeTabId === nextActiveTabId || this.activeTabSyncQueued) {
      return;
    }

    this.activeTabSyncQueued = true;
    queueMicrotask(() => {
      this.activeTabSyncQueued = false;
      if (this.destroyed) {
        return;
      }

      this.activeTabId = this.getCurrentActiveTabId();
      this.hostChangeDetectorRef.detectChanges();

      if (this.showContent) {
        this.queueRenderExtension();
      } else {
        this.destroyExtensionElement();
      }
    });
  }

  private getCurrentActiveTabId(): string {
    return this.topBarService.activeTopBarItem?.id ?? 'content';
  }

  private updateExtensionContext(): void {
    if (!this.extensionElement || !this.extensionMetadata?.extensionKey) {
      return;
    }

    const context: OipExtensionHostContext = {
      moduleInstanceId: this.id,
      extensionKey: this.extensionMetadata.extensionKey,
      apiBasePath: this.buildUrl(`api/extensions/${this.extensionMetadata.extensionKey}`),
      settings: this.settings,
      locale: this.layoutService.language(),
      theme: this.layoutService.layoutConfig(),
      user: this.securityService.getCurrentUser(),
      activeTabId: this.topBarService.activeId ?? 'content',
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

  private buildUrl(path: string): string {
    const baseUrl = document.getElementsByTagName('base')[0]?.href ?? `${window.location.origin}/`;
    return `${baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`}${path}`;
  }

  private async loadExtensionTranslations(metadata: OipExtensionModuleMetadata): Promise<void> {
    const extensionKey = metadata.extensionKey ?? this.extensionKey;
    const apiBaseUrl = metadata.apiBaseUrl;
    if (!extensionKey || !apiBaseUrl) {
      return;
    }

    const lang = this.translateService.currentLang || this.layoutService.language() || 'en';
    const url = this.buildExtensionI18nUrl(apiBaseUrl, extensionKey, lang);
    try {
      await firstValueFrom(this.l10nService.loadTranslationsFromUrl(extensionKey, url, lang));
    } catch (error) {
      console.error(`No translations found for extension ${extensionKey}.${lang}.json`, error);
    }
  }

  private buildExtensionI18nUrl(apiBaseUrl: string, extensionKey: string, lang: string): string {
    return `${apiBaseUrl.replace(/\/+$/, '')}/assets/i18n/${encodeURIComponent(extensionKey)}.${encodeURIComponent(lang)}.json`;
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

  private async renderCustomElementExtension(metadata: OipExtensionModuleMetadata): Promise<void> {
    const {elementName, scriptUrl} = metadata;
    if (!this.container || !elementName || !scriptUrl) {
      this.loadError = 'Custom Element extension metadata is incomplete.';
      return;
    }

    await this.extensionLoader.loadScript(scriptUrl);
    await customElements.whenDefined(elementName);
    const element = document.createElement(elementName);
    this.extensionElement = element;
    this.attachListeners(element);
    this.container.nativeElement.appendChild(element);
    this.updateExtensionContext();
  }
}
