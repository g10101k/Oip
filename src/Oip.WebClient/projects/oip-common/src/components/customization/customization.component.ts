import { Component, computed, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { CustomizationApi } from '../../api/customization.api';
import { SiteCustomization } from '../../api/data-contracts';
import { provideTranslations } from '../../helpers/l10n.helper';
import { AppInfoService } from '../../services/app-info.service';
import { MsgService } from '../../services/msg.service';
import { LogoComponent } from '../logo/logo.component';
import en from './l10n/customization.en.json';
import ru from './l10n/customization.ru.json';

@Component({
  selector: 'app-customization',
  imports: [FormsModule, TranslatePipe, ButtonModule, TabsModule, TextareaModule, LogoComponent],
  providers: [CustomizationApi],
  host: { class: 'flex min-w-0 flex-1 flex-col' },
  templateUrl: './customization.component.html',
  styleUrl: './customization.component.css',
  // The layout container is outside this component's template.
  encapsulation: ViewEncapsulation.None
})
export class CustomizationComponent implements OnInit {
  private readonly api = inject(CustomizationApi);
  private readonly messages = inject(MsgService);
  private readonly translate = inject(TranslateService);
  private readonly translations = provideTranslations({ en, ru });
  private readonly saved = signal<SiteCustomization | null>(null);
  private readonly logoChanged = computed(
    () => (this.customization()?.logoBase64 ?? null) !== (this.saved()?.logoBase64 ?? null)
  );

  protected readonly appInfo = inject(AppInfoService);
  protected readonly customization = signal<SiteCustomization | null>(null);
  protected readonly busy = signal(true);
  protected readonly maxTextLength = 512 * 1024;
  protected readonly hasChanges = computed(() => {
    const draft = this.customization();
    const saved = this.saved();
    return (
      draft !== null &&
      saved !== null &&
      (draft.css !== saved.css ||
        draft.javaScript !== saved.javaScript ||
        (draft.faviconBase64 || null) !== (saved.faviconBase64 || null) ||
        this.logoChanged())
    );
  });
  protected readonly faviconPreview = computed(() => {
    const favicon = this.customization()?.faviconBase64;
    if (!favicon) {
      return 'assets/favicon.svg';
    }
    return this.imageUrl(favicon, atob(favicon.slice(0, 8)).startsWith('\x00\x00\x01\x00'));
  });
  protected readonly logoPreview = computed(() => {
    const logo = this.customization()?.logoBase64;
    return logo ? this.imageUrl(logo) : null;
  });

  async ngOnInit(): Promise<void> {
    try {
      const assets = await this.api.getCustomization();
      this.saved.set(assets);
      this.customization.set(assets);
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.loadError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected updateAssets(changes: Partial<SiteCustomization>): void {
    this.customization.update((assets) => assets && { ...assets, ...changes });
  }

  protected async selectFavicon(input: HTMLInputElement): Promise<void> {
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    if (file.size > 256 * 1024) {
      this.messages.error(this.translate.instant('customization.faviconSize'));
      return;
    }

    this.busy.set(true);
    try {
      this.updateAssets({ faviconBase64: await this.readBase64(file) });
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.faviconError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async selectLogo(input: HTMLInputElement): Promise<void> {
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }
    if (file.size > 256 * 1024) {
      this.messages.error(this.translate.instant('customization.logoSize'));
      return;
    }

    this.busy.set(true);
    try {
      const base64 = await this.readBase64(file);
      const image = new Image();
      image.src = this.imageUrl(base64);
      await image.decode();
      this.updateAssets({ logoBase64: base64 });
    } catch {
      this.messages.error(this.translate.instant('customization.logoError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async save(): Promise<void> {
    const customization = this.customization();
    if (!customization || this.busy() || !this.hasChanges()) {
      return;
    }
    this.busy.set(true);
    try {
      await this.api.updateCustomization(customization);
      this.saved.set(customization);
      this.messages.success(this.translate.instant('customization.saved'));
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.saveError'));
    } finally {
      this.busy.set(false);
    }
  }

  private async readBase64(file: File): Promise<string> {
    const reader = new FileReader();
    const url = await new Promise<string>((resolve, reject) => {
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
    return url.slice(url.indexOf(',') + 1);
  }

  private imageUrl(base64: string, isIco = false): string {
    const mimeType = isIco ? 'image/x-icon' : base64.startsWith('iVBORw0KGgo') ? 'image/png' : 'image/svg+xml';
    return `data:${mimeType};base64,${base64}`;
  }
}
