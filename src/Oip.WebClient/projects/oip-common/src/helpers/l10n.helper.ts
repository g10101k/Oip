import { inject, isDevMode } from '@angular/core';
import { L10nService, TranslationsByLang } from '../services/l10n.service';
import { Observable, of } from 'rxjs';
import { Translation, TranslationObject } from '@ngx-translate/core';

/**
 * Registers a component's co-located translations and merges them into the
 * active language. The namespace is derived from the JSON's own root key,
 * so callers never repeat it as a string literal.
 *
 * Must be called from an injection context - as a field initializer, or as
 * the first statement in a constructor - the same requirement as `inject()`
 * itself, which it uses internally.
 *
 * ```ts
 * import en from './l10n/menu.en.json';
 * import ru from './l10n/menu.ru.json';
 *
 * @Component({ ... })
 * export class MenuComponent {
 *   private readonly translations = provideTranslations({ en, ru });
 * }
 * ```
 */
export function provideTranslations(byLang: TranslationsByLang): Observable<Translation | TranslationObject> {
  if (isDevMode()) {
    warnOnKeyMismatch(byLang);
  }

  L10nService.registerTranslations(byLang);

  const namespace = namespaceOf(byLang);
  return namespace ? inject(L10nService).get(namespace) : of({});
}

function namespaceOf(byLang: TranslationsByLang): string | undefined {
  const firstLangDict = Object.values(byLang)[0];
  return firstLangDict ? Object.keys(firstLangDict)[0] : undefined;
}

/**
 * Dev-only sanity check: warns when the language dictionaries in `byLang`
 * don't all declare the same set of keys - a common source of a translated
 * string silently falling back to its raw key in one language only.
 */
function warnOnKeyMismatch(byLang: TranslationsByLang): void {
  const langs = Object.keys(byLang);
  if (langs.length < 2) {
    return;
  }

  const keysByLang = new Map(langs.map((lang) => [lang, collectKeyPaths(byLang[lang])]));
  const [baseLang, ...otherLangs] = langs;
  const baseKeys = keysByLang.get(baseLang)!;
  const namespace = namespaceOf(byLang) ?? '(unknown namespace)';

  for (const lang of otherLangs) {
    const langKeys = keysByLang.get(lang)!;
    const missing = [...baseKeys].filter((key) => !langKeys.has(key));
    const extra = [...langKeys].filter((key) => !baseKeys.has(key));

    if (missing.length > 0) {
      console.warn(
        `[provideTranslations] "${namespace}": "${lang}" is missing keys present in "${baseLang}": ${missing.join(', ')}`
      );
    }
    if (extra.length > 0) {
      console.warn(
        `[provideTranslations] "${namespace}": "${lang}" has extra keys not present in "${baseLang}": ${extra.join(', ')}`
      );
    }
  }
}

/**
 * Recursively collects dot-joined paths to every leaf value in an object,
 * e.g. { a: { b: 'x' } } -> ['a.b'].
 */
function collectKeyPaths(value: unknown, prefix = '', paths: Set<string> = new Set()): Set<string> {
  if (value !== null && typeof value === 'object' && !Array.isArray(value)) {
    for (const [key, child] of Object.entries(value as Record<string, unknown>)) {
      collectKeyPaths(child, prefix ? `${prefix}.${key}` : key, paths);
    }
  } else {
    paths.add(prefix);
  }

  return paths;
}
