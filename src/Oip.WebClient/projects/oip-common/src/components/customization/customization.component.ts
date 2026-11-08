import { Component, computed, inject, OnInit, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { CustomizationApi } from '../../api/customization.api';
import { SiteCustomization } from '../../api/data-contracts';
import { provideTranslations } from '../../helpers/l10n.helper';
import { MsgService } from '../../services/msg.service';
import en from './l10n/customization.en.json';
import ru from './l10n/customization.ru.json';

@Component({
  selector: 'app-customization',
  imports: [FormsModule, TranslatePipe, ButtonModule, TabsModule, TextareaModule],
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
        (draft.faviconBase64 || null) !== (saved.faviconBase64 || null))
    );
  });
  protected readonly faviconPreview = computed(() => {
    const favicon = this.customization()?.faviconBase64;
    if (!favicon) {
      return 'assets/favicon.svg';
    }
    const mimeType = favicon.startsWith('iVBORw0KGgo') ? 'image/png' : 'image/x-icon';
    return `data:${mimeType};base64,${favicon}`;
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
      const reader = new FileReader();
      const dataUrl = await new Promise<string>((resolve, reject) => {
        reader.onload = () => resolve(reader.result as string);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
      });
      const faviconBase64 = dataUrl.slice(dataUrl.indexOf(',') + 1);
      this.updateAssets({ faviconBase64 });
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.faviconError'));
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
}
