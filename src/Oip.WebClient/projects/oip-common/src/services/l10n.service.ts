import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { InterpolationParameters, TranslateService, Translation, TranslationObject } from '@ngx-translate/core';
import { LayoutService } from './app.layout.service';
import { PrimeNG } from 'primeng/config';
import { Observable, of, shareReplay, tap } from 'rxjs';

export interface LanguageDto {
  code: string;
  name: string;
  icon: string;
}

/**
 * Service for managing translation loading in the application
 */
@Injectable({ providedIn: 'root' }) // Provided at root level for singleton usage
export class L10nService {
  private loadedTranslations: Set<string> = new Set();
  private loadingTranslations: Map<string, Observable<unknown>> = new Map();
  private httpClient = inject(HttpClient);
  private translateService = inject(TranslateService);
  private readonly primeNg = inject(PrimeNG);
  private readonly layoutService = inject(LayoutService);
  public availableLanguages: LanguageDto[];

  /**
   * Loads translations for a specific component
   * @param component - Name of the component to load translations for
   */
  public loadComponentTranslations(component: string): Observable<unknown> {
    const lang = this.translateService.currentLang || this.layoutService.language() || 'en';
    return this.loadTranslations(component, lang);
  }

  /**
   * Loads translations from an explicit asset URL and merges them into the active language dictionary.
   * Use this for extension modules whose assets are hosted outside the shell application.
   */
  public loadTranslationsFromUrl(namespace: string, url: string, lang?: string): Observable<unknown> {
    const selectedLang = lang || this.translateService.currentLang || this.layoutService.language() || 'en';
    const key = `${namespace}.${selectedLang}.${url}`;
    if (this.loadedTranslations.has(key)) {
      return of(null);
    }

    const loading = this.loadingTranslations.get(key);
    if (loading) {
      return loading;
    }

    const request = this.httpClient.get(url).pipe(
      tap((translations) => {
        const current = this.translateService.translations[selectedLang] || {};
        this.translateService.setTranslation(selectedLang, { ...current, ...translations }, true);
        this.loadedTranslations.add(key);
        this.loadingTranslations.delete(key);
      }),
      shareReplay(1)
    );

    this.loadingTranslations.set(key, request);
    request.subscribe({
      error: (e) => {
        this.loadingTranslations.delete(key);
        console.error(`No translations found for ${namespace}.${selectedLang}.json at ${url}`);
        console.error(e);
      }
    });

    return request;
  }

  /**
   * Gets the translated value of a key (or an array of keys)
   * @returns the translated key, or an object of translated keys
   */
  public get(key: string) {
    this.loadComponentTranslations(key.split('.')[0]);
    return this.translateService.get(key);
  }

  /**
   * Internal method to load translations from JSON files
   * @param component - Component or translation namespace
   * @param lang - Language code to load translations for
   */
  private loadTranslations(component: string, lang: string): Observable<unknown> {
    const key = `${component}.${lang}`;
    if (this.loadedTranslations.has(key)) {
      return of(null);
    }

    const loading = this.loadingTranslations.get(key);
    if (loading) {
      return loading;
    }

    const request = this.httpClient.get(`./assets/i18n/${component}.${lang}.json`).pipe(
      tap((translations) => {
        const current = this.translateService.translations[lang] || {};
        this.translateService.setTranslation(lang, { ...current, ...translations }, true);
        this.loadedTranslations.add(key);
        this.loadingTranslations.delete(key);
      }),
      shareReplay(1)
    );

    this.loadingTranslations.set(key, request);
    request.subscribe({
      error: (e) => {
        this.loadingTranslations.delete(key);
        console.error(`No translations found for ${component}.${lang}.json`);
        console.error(e);
      }
    });

    return request;
  }

  /**
   * Changes the lang currently used
   */
  use(selectedLanguage: string, key: string = null) {
    if (key) {
      this.get(key);
    }
    this.translateService.use(selectedLanguage);
  }

  init(languages: LanguageDto[]) {
    this.availableLanguages = languages;
    this.translateService.addLangs(languages.map((x) => x.code));
    const lang = this.layoutService.language() ? this.layoutService.language() : 'en';
    this.translateService.setDefaultLang(lang);
    this.translateService.use(lang).subscribe(() => {
      this.loadComponentTranslations('app-info');
      this.translateService.get('primeng').subscribe((res) => this.primeNg.setTranslation(res));
    });
  }

  instant(key: string | string[], interpolateParams?: InterpolationParameters): Translation | TranslationObject {
    return this.translateService.instant(key, interpolateParams);
  }
}
