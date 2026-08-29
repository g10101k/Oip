import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { InterpolationParameters, TranslateService, Translation, TranslationObject } from '@ngx-translate/core';
import { LayoutService } from './app.layout.service';
import { PrimeNG } from 'primeng/config';
import { Observable, of, shareReplay, tap } from 'rxjs';
import en from './l10n/en.json';
import ru from './l10n/ru.json';

/**
 * Translations of a single namespace grouped by language code, e.g. { en: {...}, ru: {...} }
 */
export type TranslationsByLang = Record<string, TranslationObject>;

/**
 * Global dictionaries shared by all components. They are bundled with the library,
 * so the application never requests `assets/i18n/{lang}.json`.
 */
const globalTranslations: TranslationsByLang = { en, ru };

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
   * Translations bundled with components, registered at module load time.
   */
  private static readonly staticTranslations: Map<string, TranslationsByLang> = new Map();

  /**
   * Registers translations bundled with a component instead of loading them over HTTP.
   * Namespaces are taken from the root keys of the passed dictionaries.
   * Components derived from <c>BaseModuleComponent</c> don't call it directly - it is enough
   * to declare the static <c>translations</c> field.
   * @param byLang - Translations grouped by language code
   * @param namespace - Explicit namespace, needed when the dictionaries are still empty
   */
  public static registerTranslations(byLang: TranslationsByLang | undefined, namespace?: string): void {
    if (!byLang) {
      return;
    }
    if (namespace) {
      L10nService.staticTranslations.set(namespace, byLang);
    }
    for (const translations of Object.values(byLang)) {
      for (const namespace of Object.keys(translations)) {
        L10nService.staticTranslations.set(namespace, byLang);
      }
    }
  }

  constructor() {
    // Global dictionaries are available before the first `use()` call, so TranslateHttpLoader is never asked for them.
    L10nService.registerTranslations(globalTranslations);
    Object.entries(globalTranslations).forEach(([lang, translations]) => this.mergeTranslation(lang, translations));

    // Static translations are merged once per language, so they have to be re-merged on every language change.
    this.translateService.onLangChange.subscribe((event) => {
      L10nService.staticTranslations.forEach((byLang) => {
        const translations = byLang[event.lang];
        if (translations) {
          this.mergeTranslation(event.lang, translations);
        }
      });
    });
  }

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
        this.mergeTranslation(selectedLang, translations as TranslationObject);
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
   * Internal method to merge a namespace's statically-registered translations
   * (see <c>registerTranslations</c>) into the active language dictionary.
   * @param component - Translation namespace
   * @param lang - Language code to load translations for
   */
  private loadTranslations(component: string, lang: string): Observable<unknown> {
    const key = `${component}.${lang}`;
    if (this.loadedTranslations.has(key)) {
      return of(null);
    }

    const staticTranslations = L10nService.staticTranslations.get(component);
    if (!staticTranslations) {
      console.error(`No translations registered for namespace "${component}".`);
      return of(null);
    }

    const translations = staticTranslations[lang];
    if (translations) {
      this.mergeTranslation(lang, translations);
    }
    this.loadedTranslations.add(key);

    return of(null);
  }

  /**
   * Merges a translation dictionary into the dictionary of the given language
   * @param lang - Language code
   * @param translations - Translations to merge
   */
  private mergeTranslation(lang: string, translations: TranslationObject): void {
    const current = this.translateService.translations[lang] || {};
    this.translateService.setTranslation(lang, { ...current, ...translations }, true);
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
