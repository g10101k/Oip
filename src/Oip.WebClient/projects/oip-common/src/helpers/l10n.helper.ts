import { inject } from '@angular/core';
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
  L10nService.registerTranslations(byLang);

  const namespace = namespaceOf(byLang);
  return namespace ? inject(L10nService).get(namespace) : of({});
}

function namespaceOf(byLang: TranslationsByLang): string | undefined {
  const firstLangDict = Object.values(byLang)[0];
  return firstLangDict ? Object.keys(firstLangDict)[0] : undefined;
}
