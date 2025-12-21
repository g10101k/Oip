import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { LayoutService } from "./app.layout.service";
import { PrimeNG } from "primeng/config";

/**
 * Service for managing translation loading in the application
 */
@Injectable({ providedIn: 'root' }) // Provided at root level for singleton usage
export class L10nService {
  private loadedTranslations: Set<string> = new Set();
  private httpClient = inject(HttpClient);
  private translateService = inject(TranslateService);
  private readonly primeNg = inject(PrimeNG);
  private readonly layoutService = inject(LayoutService);

  /**
   * Loads translations for a specific component
   * @param component - Name of the component to load translations for
   */
  public loadComponentTranslations(component: string) {
    const lang = this.translateService.currentLang;
    this.loadTranslations(component, lang);
  }

  /**
   * Gets the translated value of a key (or an array of keys)
   * @returns the translated key, or an object of translated keys
   */
  public get(key: string) {
    this.loadComponentTranslations(key)
    return this.translateService.get(key);
  }

  /**
   * Internal method to load translations from JSON files
   * @param component - Component or translation namespace
   * @param lang - Language code to load translations for
   */
  private loadTranslations(component: string, lang: string) {
    // Create unique key for this component-language combination
    const key = `${component}.${lang}`;

    // Skip if translations are already loaded
    if (this.loadedTranslations.has(key)) {
      return;
    }

    try {
      // Load translation file from assets
      this.httpClient.get(`./assets/i18n/${component}.${lang}.json`).subscribe(
        (translations) => {
          // Get existing translations for the language
          const current = this.translateService.translations[lang] || {};

          // Merge new translations with existing ones
          this.translateService.setTranslation(lang, { ...current, ...translations }, true);

          // Mark these translations as loaded
          this.loadedTranslations.add(key);
        }
      );


    } catch (e) {
      console.warn(`No translations found for ${component}.${lang}.json`);
      console.error(e);
    }
  }

  /**
   * Changes the lang currently used
   */
  use(selectedLanguage: string, key: string = null) {
    if (key) {
      this.get(key);
    }
    this.translateService.use(selectedLanguage)
  }

  init(langs: string[]) {
    this.translateService.addLangs(langs);
    const lang = /en|ru/.exec(this.layoutService.language()) ? this.layoutService.language() : 'en';
    this.translateService.setDefaultLang(lang);
    this.translateService.use(lang);
    this.loadComponentTranslations('app-info');
    this.translateService.get('primeng').subscribe((res) => this.primeNg.setTranslation(res));
  }
}
