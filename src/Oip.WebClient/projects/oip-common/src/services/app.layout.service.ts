import { Injectable, effect, signal, computed } from '@angular/core';
import { Subject } from 'rxjs';
import { convertToPrimeNgDateFormat } from '../helpers/date.helper';

export interface AppConfig {
  preset?: string;
  primary?: string;
  surface?: string | null;
  darkTheme?: boolean;
  menuMode?: string;
  language?: string;
  dateFormat: string;
  timeFormat: string;
  dateTimeFormat: string;
  timeZone: string;
}

interface LayoutState {
  staticMenuDesktopInactive?: boolean;
  overlayMenuActive?: boolean;
  configSidebarVisible?: boolean;
  staticMenuMobileActive?: boolean;
  menuHoverActive?: boolean;
}

interface MenuChangeEvent {
  key: string;
  routeEvent?: boolean;
}

@Injectable({ providedIn: 'root' })
export class LayoutService {
  _config: AppConfig = this.getAppConfigFromStorage();

  _state: LayoutState = {
    staticMenuDesktopInactive: false,
    overlayMenuActive: false,
    configSidebarVisible: false,
    staticMenuMobileActive: false,
    menuHoverActive: false
  };

  layoutConfig = signal<AppConfig>(this._config);

  layoutState = signal<LayoutState>(this._state);

  private readonly configUpdate = new Subject<AppConfig>();

  private readonly overlayOpen = new Subject<any>();

  private readonly menuSource = new Subject<MenuChangeEvent>();

  private readonly resetSource = new Subject();

  menuSource$ = this.menuSource.asObservable();

  resetSource$ = this.resetSource.asObservable();

  configUpdate$ = this.configUpdate.asObservable();

  overlayOpen$ = this.overlayOpen.asObservable();

  theme = computed(() => (this.layoutConfig()?.darkTheme ? 'light' : 'dark'));

  isSidebarActive = computed(() => this.layoutState().overlayMenuActive || this.layoutState().staticMenuMobileActive);

  isDarkTheme = computed(() => this.layoutConfig().darkTheme);

  getPrimary = computed(() => this.layoutConfig().primary);

  getSurface = computed(() => this.layoutConfig().surface);

  isOverlay = computed(() => this.layoutConfig().menuMode === 'overlay');

  language = computed(() => this.layoutConfig().language);

  dateFormat = computed(() => this.layoutConfig().dateFormat);

  primeNgDateFormat = computed(() => convertToPrimeNgDateFormat(this.layoutConfig().dateFormat));

  timeFormat = computed(() => this.layoutConfig().timeFormat);

  dateTimeFormat = computed(() => `${this.layoutConfig().dateFormat} ${this.layoutConfig().timeFormat}`);

  monthFormat = computed(() => {
    const reDay = /d+/i;
    const reDelimeter = /^[^\w]|[^\w]$|([^\w])\1+/;
    const ngDateFormat = convertToPrimeNgDateFormat(this.layoutConfig().dateFormat);
    const ngDate = ngDateFormat.replace(reDay, '');
    const dateGroups = ngDate.match(reDelimeter);
    if (Array.isArray(dateGroups) && dateGroups.length > 1) {
      return dateGroups[1] !== undefined
        ? ngDate.replace(dateGroups[0], '')
        : ngDate.startsWith(dateGroups[0])
          ? ngDate.substring(1)
          : ngDate.substring(0, ngDate.length - 1);
    }
    return ngDateFormat;
  });

  timeZone = computed(() => this.layoutConfig().timeZone);

  transitionComplete = signal<boolean>(false);

  private initialized = false;

  constructor() {
    effect(() => {
      const config = this.layoutConfig();
      if (config) {
        this.onConfigUpdate();
      }
    });

    effect(() => {
      const config = this.layoutConfig();

      if (!this.initialized || !config) {
        this.initialized = true;
        return;
      }

      this.handleDarkModeTransition(config);
    });
  }

  /**
   * Get application settings from browser storage
   * @returns AppConfig
   */
  private getAppConfigFromStorage(): AppConfig {
    const appConfigUiString = localStorage.getItem('layoutConfig');
    if (appConfigUiString != null) {
      const config = JSON.parse(appConfigUiString) as AppConfig;
      config.timeZone ??= Intl.DateTimeFormat().resolvedOptions().timeZone;
      return config;
    }
    return {
      preset: 'Aura',
      primary: 'emerald',
      surface: null,
      darkTheme: false,
      menuMode: 'static',
      language: 'ru',
      dateFormat: 'yyyy-MM-dd',
      timeFormat: 'HH:mm:ss',
      dateTimeFormat: 'yyyy-MM-dd HH:mm:ss',
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone
    };
  }

  private handleDarkModeTransition(config: AppConfig): void {
    if ((document as any).startViewTransition) {
      this.startViewTransition(config);
    } else {
      this.toggleDarkMode(config);
      this.onTransitionEnd();
    }
  }

  private startViewTransition(config: AppConfig): void {
    const transition = (document as any).startViewTransition(() => {
      this.toggleDarkMode(config);
    });

    transition.ready
      .then(() => {
        this.onTransitionEnd();
      })
      .catch(() => {});
  }

  toggleDarkMode(config?: AppConfig): void {
    const _config = config || this.layoutConfig();
    if (_config.darkTheme) {
      document.documentElement.classList.add('app-dark');
    } else {
      document.documentElement.classList.remove('app-dark');
    }
  }

  private onTransitionEnd() {
    this.transitionComplete.set(true);
    setTimeout(() => {
      this.transitionComplete.set(false);
    });
  }

  onMenuToggle() {
    if (this.isOverlay()) {
      this.layoutState.update((prev) => ({ ...prev, overlayMenuActive: !this.layoutState().overlayMenuActive }));

      if (this.layoutState().overlayMenuActive) {
        this.overlayOpen.next(null);
      }
    }

    if (this.isDesktop()) {
      this.layoutState.update((prev) => ({
        ...prev,
        staticMenuDesktopInactive: !this.layoutState().staticMenuDesktopInactive
      }));
    } else {
      this.layoutState.update((prev) => ({
        ...prev,
        staticMenuMobileActive: !this.layoutState().staticMenuMobileActive
      }));

      if (this.layoutState().staticMenuMobileActive) {
        this.overlayOpen.next(null);
      }
    }
  }

  isDesktop() {
    return window.innerWidth > 991;
  }

  isMobile() {
    return !this.isDesktop();
  }

  onConfigUpdate() {
    this._config = { ...this.layoutConfig() };
    this.configUpdate.next(this.layoutConfig());
    localStorage.setItem('layoutConfig', JSON.stringify(this.layoutConfig()));
  }

  onMenuStateChange(event: MenuChangeEvent) {
    this.menuSource.next(event);
  }

  reset() {
    this.resetSource.next(true);
  }
}
