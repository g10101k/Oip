import { CommonModule } from '@angular/common';
import {
  Component,
  ComponentRef,
  EnvironmentInjector,
  inject,
  Injector,
  OnDestroy,
  Type,
  ViewChild,
  ViewContainerRef
} from '@angular/core';
import { loadRemoteModule } from '@angular-architects/module-federation';
import { Button } from 'primeng/button';
import { firstValueFrom } from 'rxjs';
import { BaseModuleComponent } from './base-module.component';
import { emitOipContextChange } from '../extension-host/extension-host.events';
import { ExtensionModulesApi } from '../api/extension-modules.api';
import { OipExtensionHostContext, OipExtensionModuleMetadata } from '../extension-host/extension-host.types';
import { CustomElementExtensionModuleHostComponent } from "./custom-element-extension-module-host.component";

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

    await this.loadExtensionTranslations(this.extensionMetadata);
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

  private async loadExtensionTranslations(metadata: OipExtensionModuleMetadata): Promise<void> {
    const extensionKey = metadata.extensionKey ?? this.extensionKey;
    const apiBaseUrl = metadata.apiBaseUrl;
    if (!extensionKey || !apiBaseUrl) {
      return;
    }

    const lang = this.translateService.currentLang || this.layoutService.language() || 'en';
    for (const namespace of this.getExtensionTranslationNamespaces(metadata, extensionKey)) {
      const url = this.buildExtensionI18nUrl(apiBaseUrl, namespace, lang);
      try {
        await firstValueFrom(this.l10nService.loadTranslationsFromUrl(namespace, url, lang));
        return;
      } catch (error) {
        console.error(`No translations found for extension ${namespace}.${lang}.json`, error);
      }
    }
  }

  private buildExtensionI18nUrl(apiBaseUrl: string, extensionKey: string, lang: string): string {
    return `${apiBaseUrl.replace(/\/+$/, '')}/assets/i18n/${encodeURIComponent(extensionKey)}.${encodeURIComponent(lang)}.json`;
  }

  private getExtensionTranslationNamespaces(metadata: OipExtensionModuleMetadata, extensionKey: string): string[] {
    const namespaces = [extensionKey];
    const componentNamespace = this.toKebabCase(metadata.componentName?.replace(/Component$/, '') ?? '');
    if (componentNamespace && !namespaces.includes(componentNamespace)) {
      namespaces.push(componentNamespace);
    }

    return namespaces;
  }

  private toKebabCase(value: string): string {
    return value
      .replace(/([a-z0-9])([A-Z])/g, '$1-$2')
      .replace(/[\s_]+/g, '-')
      .toLowerCase();
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
