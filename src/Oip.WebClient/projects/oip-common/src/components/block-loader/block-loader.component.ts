import { Component, effect, inject, OnDestroy, signal, untracked } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { ProgressSpinner } from 'primeng/progressspinner';
import { provideTranslations } from '../../helpers/l10n.helper';
import { ModuleLoadingService } from '../../services/module-loading.service';

import en from './l10n/block-loader.en.json';
import ru from './l10n/block-loader.ru.json';

/**
 * Full screen blocker shown while the application switches between modules.
 *
 * Render it as a sibling of `.layout-wrapper`, not inside it: while the blocker is visible the
 * wrapper is marked `inert`, which would also disable an overlay nested in it.
 *
 * The blocker appears only when a transition outlasts {@link showDelayMs} and then stays for at
 * least {@link minVisibleMs}, so quick navigation does not flash a spinner.
 */
@Component({
  selector: 'block-loader',
  standalone: true,
  imports: [ProgressSpinner, TranslatePipe],
  template: `
    @if (visible()) {
      <div
        class="animate-fadein fixed inset-0 z-[1200] flex flex-col items-center justify-center gap-4 bg-surface-0/70 dark:bg-surface-900/70"
        role="status"
        aria-live="polite"
        aria-busy="true"
        (pointerdown)="$event.preventDefault()"
        (contextmenu)="$event.preventDefault()">
        <p-progress-spinner [style]="{ width: '3rem', height: '3rem' }" animationDuration=".8s" strokeWidth="4" />
        <span class="text-color font-medium">{{ 'blockLoader.loading' | translate }}</span>
      </div>
    }
  `
})
export class BlockLoaderComponent implements OnDestroy {
  private static readonly showDelayMs = 150;
  private static readonly minVisibleMs = 300;

  private readonly translations = provideTranslations({ en, ru });
  private readonly moduleLoadingService = inject(ModuleLoadingService);

  private showHandle?: ReturnType<typeof setTimeout>;
  private hideHandle?: ReturnType<typeof setTimeout>;
  private shownAt = 0;

  protected readonly visible = signal(false);

  constructor() {
    effect(() => {
      const loading = this.moduleLoadingService.loading();
      untracked(() => (loading ? this.scheduleShow() : this.scheduleHide()));
    });

    effect(() => {
      this.blockInteraction(this.visible());
    });
  }

  ngOnDestroy(): void {
    this.clearTimers();
    this.blockInteraction(false);
  }

  private scheduleShow(): void {
    this.clearTimer('hide');
    if (this.visible() || this.showHandle != null) {
      return;
    }

    this.showHandle = setTimeout(() => {
      this.showHandle = undefined;
      this.shownAt = Date.now();
      this.visible.set(true);
    }, BlockLoaderComponent.showDelayMs);
  }

  private scheduleHide(): void {
    this.clearTimer('show');
    if (!this.visible() || this.hideHandle != null) {
      return;
    }

    const remaining = BlockLoaderComponent.minVisibleMs - (Date.now() - this.shownAt);
    if (remaining <= 0) {
      this.visible.set(false);
      return;
    }

    this.hideHandle = setTimeout(() => {
      this.hideHandle = undefined;
      this.visible.set(false);
    }, remaining);
  }

  private clearTimer(timer: 'show' | 'hide'): void {
    const handle = timer === 'show' ? this.showHandle : this.hideHandle;
    if (handle == null) {
      return;
    }

    clearTimeout(handle);
    if (timer === 'show') {
      this.showHandle = undefined;
    } else {
      this.hideHandle = undefined;
    }
  }

  private clearTimers(): void {
    this.clearTimer('show');
    this.clearTimer('hide');
  }

  /**
   * Takes the application out of the interaction and accessibility trees while the blocker is up,
   * so pointer, keyboard and screen reader input cannot reach a module that is still loading.
   */
  private blockInteraction(blocked: boolean): void {
    const wrapper = document.querySelector('.layout-wrapper');
    if (blocked) {
      wrapper?.setAttribute('inert', '');
      // Not the layout's `blocked-scroll` class: the layout drops it whenever it closes the menu.
      document.body.style.setProperty('overflow', 'hidden');
    } else {
      wrapper?.removeAttribute('inert');
      document.body.style.removeProperty('overflow');
    }
  }
}
