import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { computed, effect, inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { AppState } from "../appstate";
import { Subject } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {
  private readonly STORAGE_KEY = 'appConfigState';

  appState = signal<AppState>(null);

  designerActive = signal(false);

  newsActive = signal(false);

  document = inject(DOCUMENT);

  platformId = inject(PLATFORM_ID);

  theme = computed(() => (this.appState()?.darkTheme ? 'dark' : 'light'));

  transitionComplete = signal<boolean>(false);

  private initialized = false;

  private configUpdate = new Subject<AppState>();

  private overlayOpen = new Subject<any>();

  configUpdate$ = this.configUpdate.asObservable();

  overlayOpen$ = this.overlayOpen.asObservable();

  constructor() {
    this.appState.set({ ...this.loadAppState() });

    effect(() => {
      const state = this.appState();

      if (!this.initialized || !state) {
        this.initialized = true;
        return;
      }
      this.saveAppState(state);
      this.handleDarkModeTransition(state);
    });
  }

  private handleDarkModeTransition(state: AppState): void {
    if (isPlatformBrowser(this.platformId)) {
      if ((document as any).startViewTransition) {
        this.startViewTransition(state);
      } else {
        this.toggleDarkMode(state);
        this.onTransitionEnd();
      }
    }
  }

  private startViewTransition(state: AppState): void {
    const transition = (document as any).startViewTransition(() => {
      this.toggleDarkMode(state);
    });

    transition.ready.then(() => this.onTransitionEnd());
  }

  private toggleDarkMode(state: AppState): void {
    if (state.darkTheme) {
      this.document.documentElement.classList.add('p-dark');
    } else {
      this.document.documentElement.classList.remove('p-dark');
    }
  }

  private onTransitionEnd() {
    this.transitionComplete.set(true);
    setTimeout(() => {
      this.transitionComplete.set(false);
    });
  }

  hideMenu() {
    this.appState.update((state) => ({
      ...state,
      menuActive: false
    }));
  }

  showMenu() {
    this.appState.update((state) => ({
      ...state,
      menuActive: true
    }));
  }

  hideNews() {
    this.newsActive.set(false);
  }

  showNews() {
    this.newsActive.set(true);
  }

  showDesigner() {
    this.designerActive.set(true);
  }

  hideDesigner() {
    this.designerActive.set(false);
  }

  private loadAppState(): any {
    if (isPlatformBrowser(this.platformId)) {
      const storedState = localStorage.getItem(this.STORAGE_KEY);
      if (storedState) {
        return JSON.parse(storedState);
      }
    }
    return {
      preset: 'Aura',
      primary: 'noir',
      surface: null,
      darkTheme: false,
      menuActive: false,
      designerKey: 'primeng-designer-theme',
      RTL: false
    };
  }

  private saveAppState(state: any): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    }
  }

  isOverlay() {
    return this.appState().menuMode === 'overlay';
  }

  onMenuToggle() {
    if (this.isOverlay()) {
      this.appState().overlayMenuActive = !this.appState().overlayMenuActive;
      if (this.appState().overlayMenuActive) {
        this.overlayOpen.next(null);
      }
    }

    if (this.isDesktop()) {
      this.appState().staticMenuDesktopInactive =
        !this.appState().staticMenuDesktopInactive;
    } else {
      this.appState().staticMenuMobileActive =
        !this.appState().staticMenuMobileActive;

      if (this.appState().staticMenuMobileActive) {
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

  showProfileSidebar() {
    this.appState().profileSidebarVisible = !this.appState().profileSidebarVisible;
    if (this.appState().profileSidebarVisible) {
      this.overlayOpen.next(null);
    }
  }

}

