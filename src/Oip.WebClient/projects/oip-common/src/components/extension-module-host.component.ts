import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  ComponentRef,
  ElementRef,
  EnvironmentInjector,
  inject,
  Injector,
  OnDestroy,
  Type,
  ViewChild,
  ViewContainerRef
} from '@angular/core';
import { Router } from '@angular/router';
import { loadRemoteModule } from '@angular-architects/module-federation';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { BaseModuleComponent } from './base-module.component';
import { SecurityComponent } from './security.component';
import { ExtensionLoaderService } from '../extension-host/extension-loader.service';
import { OIP_EXTENSION_EVENTS, emitOipContextChange } from '../extension-host/extension-host.events';
import { ExtensionModulesApi } from '../api/extension-modules.api';
import {
  OipExtensionHostContext,
  OipExtensionModuleMetadata,
  OipExtensionNavigateEvent,
  OipExtensionNotifyEvent
} from '../extension-host/extension-host.types';

@Component({
  standalone: true,
  imports: [CommonModule, Button],
  providers: [ExtensionModulesApi],
  template: `
    @if (loadError) {
      <div class="card flex flex-col gap-4">
        <div class="font-semibold text-xl">{{ loadError }}</div>
        <p-button icon="pi pi-refresh" label="Retry" (onClick)="reloadExtension()"></p-button>
      </div>
    } @else {
      <ng-container #extensionContainer></ng-container>
    }
  `
})
export class ExtensionModuleHostComponent extends BaseModuleComponent<unknown, unknown> implements OnDestroy {
  @ViewChild('extensionContainer', {read: ViewContainerRef})
  private set extensionViewContainer(viewContainer: ViewContainerRef | undefined) {
    this.viewContainer = viewContainer;
    this.queueRenderExtension();
  }

  private readonly injector = inject(Injector);
  private readonly environmentInjector = inject(EnvironmentInjector);
  private readonly extensionModulesApi = inject(ExtensionModulesApi);
  private viewContainer?: ViewContainerRef;
  private extensionComponent?: ComponentRef<unknown>;
  private extensionElement?: HTMLElement;
  private extensionMetadata?: OipExtensionModuleMetadata;
  private renderQueued = false;
  private destroyed = false;

  protected loadError: string | null = null;

  private get extensionKey(): string {
    return this.route.snapshot.paramMap.get('extensionKey') ?? '';
  }

  constructor() {
    super();
    this.subscriptions.push(
      this.topBarService.activeId$.subscribe(() => {
        this.updateExtensionContext();
      })
    );
  }

  async reloadExtension(): Promise<void> {
    this.loadError = null;
    this.extensionMetadata = undefined;
    await this.loadExtensionMetadata();
    await this.renderExtension();
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
    this.destroyExtensionComponent();
    super.ngOnDestroy();
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
  }

  private async renderExtension(): Promise<void> {
    if (this.destroyed || !this.viewContainer) {
      return;
    }

    try {
      this.loadError = null;
      await this.loadExtensionMetadata();
      if (!this.extensionMetadata) {
        return;
      }

      this.destroyExtensionComponent();

      if (this.extensionMetadata.loadType === 'moduleFederation') {
        await this.renderFederatedExtension(this.extensionMetadata);
      } else {
        this.extensionComponent = this.viewContainer.createComponent(CustomElementExtensionModuleHostComponent, {
          injector: this.injector,
          environmentInjector: this.environmentInjector
        });
      }
    } catch (error) {
      this.loadError = error instanceof Error ? error.message : 'Extension could not be loaded.';
      console.error(error);
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

  private destroyExtensionComponent(): void {
    this.extensionComponent?.destroy();
    this.extensionComponent = undefined;
    this.extensionElement = undefined;
    this.viewContainer?.clear();
  }

  private async renderFederatedExtension(metadata: OipExtensionModuleMetadata): Promise<void> {
    const {remoteEntryUrl, exposedModule, componentName} = metadata;
    if (!remoteEntryUrl || !exposedModule || !componentName) {
      this.loadError = 'Module Federation extension metadata is incomplete.';
      return;
    }

    const remoteModule = await loadRemoteModule<Record<string, Type<unknown>>>({
      type: 'module',
      remoteEntry: remoteEntryUrl,
      exposedModule
    });
    const component = remoteModule[componentName];
    if (!component) {
      throw new Error(`Remote component '${componentName}' was not exported by '${exposedModule}'.`);
    }

    this.extensionComponent = this.viewContainer?.createComponent(component, {
      environmentInjector: this.environmentInjector
    });
    this.extensionElement = this.extensionComponent?.location.nativeElement as HTMLElement | undefined;
    this.updateExtensionContext();
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

  private buildUrl(path: string): string {
    const baseUrl = document.getElementsByTagName('base')[0]?.href ?? `${window.location.origin}/`;
    return `${baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`}${path}`;
  }
}

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
