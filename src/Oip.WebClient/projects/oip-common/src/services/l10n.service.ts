import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { lastValueFrom } from 'rxjs';

/**
 * Service for managing translation loading in the application
 */
@Injectable({ providedIn: 'root' }) // Provided at root level for singleton usage
export class L10nService {
  private loadedTranslations: Set<string> = new Set();
  private httpClient = inject(HttpClient);
  private translateService = inject(TranslateService);

  /**
   * Loads translations for a specific component
   * @param component - Name of the component to load translations for
   */
  async loadComponentTranslations(component: string): Promise<void> {
    const lang = this.translateService.currentLang;
    await this.loadTranslations(component, lang);
  }

  /**
   * Gets the translated value of a key (or an array of keys)
   * @returns the translated key, or an object of translated keys
   */
  public async get(key: string) {
    await this.loadComponentTranslations(key);
    return this.translateService.get(key);
  }

  /**
   * Internal method to load translations from JSON files
   * @param component - Component or translation namespace
   * @param lang - Language code to load translations for
   */
  private async loadTranslations(component: string, lang: string): Promise<void> {
    // Create unique key for this component-language combination
    const key = `${component}.${lang}`;

    // Skip if translations are already loaded
    if (this.loadedTranslations.has(key)) {
      return;
    }

    try {
      // Load translation file from assets
      const translations = await lastValueFrom(this.httpClient.get(`./assets/i18n/${component}.${lang}.json`));

      // Get existing translations for the language
      const current = this.translateService.translations[lang] || {};

      // Merge new translations with existing ones
      this.translateService.setTranslation(lang, { ...current, ...translations }, true);

      // Mark these translations as loaded
      this.loadedTranslations.add(key);
    } catch (e) {
      console.warn(`No translations found for ${component}.${lang}.json`);
    }
  }

  /**
   * Changes the lang currently used
   */
  use(selectedLanguage: string) {
    this.translateService.use(selectedLanguage);
  }
}
