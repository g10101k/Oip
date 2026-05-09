import { Component, ElementRef, inject, OnDestroy, OnInit, Renderer2, ViewChild } from '@angular/core';
import { BaseModuleComponent } from './base-module.component';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { InputText } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { SecurityComponent } from './security.component';
import { IframeModuleSettings } from '../api/data-contracts';


@Component({
  standalone: true,
  imports: [SecurityComponent, TranslatePipe, InputText, FormsModule, Button],
  template: `
    @if (isContent) {
      <iframe
        #iframe
        class="w-full h-screen"
        style="background-color: var(--surface-ground)"
        title="Main iframe"
        (error)="onIframeError()">
      </iframe>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'baseComponent.settings' | translate }}</div>
            <div class="col-span-30 md:col-span-10 w-full">
              <input
                class="w-full"
                pInputText
                placeholder="{{ 'iframe-module.iframeModule.urlPlaceholder' | translate }}"
                qa-id="iframe-module-settings-site-url-input"
                type="text"
                [(ngModel)]="settings.url"/>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                label="{{ 'iframe-module.iframeModule.settingSaveButtonLabel' | translate }}"
                qa-id="iframe-module-settings-save-button"
                (onClick)="saveSettings(settings)">
              </p-button>
            </div>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `
})
export class IframeModuleComponent
  extends BaseModuleComponent<IframeModuleSettings, IframeModuleSettings>
  implements OnInit, OnDestroy
{
  private iframe?: ElementRef<HTMLIFrameElement>;
  private readonly renderer = inject(Renderer2);
  private readonly translate = inject(TranslateService);

  protected iframeUrl: string | null = null;

  constructor() {
    super();
    this.l10nService.loadComponentTranslations('iframe-module');
  }

  @ViewChild('iframe')
  private set iframeElement(element: ElementRef<HTMLIFrameElement> | undefined) {
    this.iframe = element;
    this.updateIframeSrc();
  }

  private setIframeUrl(url: string | null | undefined): void {
    const iframeUrl = url?.trim();

    if (!iframeUrl) {
      this.iframeUrl = null;
      this.updateIframeSrc();
      const message: string = this.translate.instant('iframe-module.iframeModule.emptyUrlMessage');
      this.msgService.warn(message);
      return;
    }

    if (!this.isAllowedIframeUrl(iframeUrl)) {
      this.iframeUrl = null;
      this.updateIframeSrc();
      const message: string = this.translate.instant('iframe-module.iframeModule.siteLoadingMessage');
      this.msgService.error(message);
      return;
    }

    this.iframeUrl = iframeUrl;
    this.updateIframeSrc();
  }

  private isAllowedIframeUrl(url: string): boolean {
    try {
      const parsedUrl = new URL(url, window.location.origin);

      return parsedUrl.protocol === 'http:' || parsedUrl.protocol === 'https:';
    } catch {
      return false;
    }
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    this.setIframeUrl(this.settings.url);
  }

  private updateIframeSrc(): void {
    if (!this.iframe) {
      return;
    }

    if (!this.iframeUrl) {
      this.renderer.removeAttribute(this.iframe.nativeElement, 'src');
      return;
    }

    this.renderer.setAttribute(this.iframe.nativeElement, 'src', this.iframeUrl);
  }

  onIframeError() {
    const errorMessage = this.translate.instant('iframe-module.iframeModule.siteLoadingMessage');
    this.msgService.error(errorMessage);
  }
}
