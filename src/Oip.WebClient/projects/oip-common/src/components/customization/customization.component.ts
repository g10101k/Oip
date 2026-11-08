import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { CustomizationApi } from '../../api/customization.api';
import { SiteCustomization } from '../../api/data-contracts';
import { provideTranslations } from '../../helpers/l10n.helper';
import { MsgService } from '../../services/msg.service';
import en from './l10n/customization.en.json';
import ru from './l10n/customization.ru.json';

@Component({
  selector: 'app-customization',
  imports: [FormsModule, TranslatePipe, ButtonModule, TextareaModule],
  providers: [CustomizationApi],
  templateUrl: './customization.component.html'
})
export class CustomizationComponent implements OnInit {
  private readonly api = inject(CustomizationApi);
  private readonly messages = inject(MsgService);
  private readonly translate = inject(TranslateService);
  private readonly translations = provideTranslations({ en, ru });

  protected readonly customization = signal<SiteCustomization | null>(null);
  protected readonly busy = signal(true);
  protected readonly maxTextLength = 512 * 1024;

  async ngOnInit(): Promise<void> {
    try {
      this.customization.set(await this.api.getCustomization());
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.loadError'));
    } finally {
      this.busy.set(false);
    }
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
      this.customization.update((value) => value && { ...value, faviconBase64 });
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.faviconError'));
    } finally {
      this.busy.set(false);
    }
  }

  protected async save(customization: SiteCustomization): Promise<void> {
    this.busy.set(true);
    try {
      await this.api.updateCustomization(customization);
      this.messages.success(this.translate.instant('customization.saved'));
    } catch (error) {
      this.messages.errorFromException(error, this.translate.instant('customization.saveError'));
    } finally {
      this.busy.set(false);
    }
  }
}
