import { inject, Injectable, InjectionToken, Provider } from '@angular/core';

/**
 * Application information value. A function or a signal is read on every change detection,
 * so it can return a localized value.
 */
export type AppInfoValue = string | (() => string);

/**
 * Application information displayed by the shell components.
 */
export interface AppInfo {
  /**
   * Application version, for example generated at build time from package.json.
   */
  version?: AppInfoValue;

  /**
   * Application name displayed in the footer.
   */
  footer?: AppInfoValue;

  /**
   * Application name displayed in the top bar.
   */
  title?: AppInfoValue;
}

export const APP_INFO_TOKEN = new InjectionToken<AppInfo>('APP_INFO_TOKEN');

/**
 * Provides application information instead of the `app-info.*` translation keys.
 */
export function provideAppInfo(appInfo: AppInfo): Provider {
  return {
    provide: APP_INFO_TOKEN,
    useValue: appInfo
  };
}

@Injectable({ providedIn: 'root' })
export class AppInfoService {
  private appInfo = inject(APP_INFO_TOKEN, { optional: true });

  /**
   * Returns the provided version or null when it should be taken from translations.
   */
  getVersion(): string | null {
    return this.resolve(this.appInfo?.version);
  }

  /**
   * Returns the provided application name or null when it should be taken from translations.
   */
  getFooter(): string | null {
    return this.resolve(this.appInfo?.footer);
  }

  /**
   * Returns the provided application name or null when it should be taken from translations.
   */
  getTitle(): string | null {
    return this.resolve(this.appInfo?.title);
  }

  private resolve(value: AppInfoValue | undefined): string | null {
    return (typeof value === 'function' ? value() : value) || null;
  }
}
