import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationSkipped,
  NavigationStart,
  Router
} from '@angular/router';

/**
 * Tracks whether the application is switching between modules.
 *
 * Two sources feed the state:
 * - router navigation, from `NavigationStart` until the navigation ends, is cancelled or fails;
 * - explicit work registered with {@link begin}/{@link end} or {@link track}, which covers everything
 *   running after `NavigationEnd`: module instance rights, settings and extension loading.
 *
 * The UI blocker rendered by the layout observes {@link loading}.
 */
@Injectable({ providedIn: 'root' })
export class ModuleLoadingService {
  /** Releases a stuck blocker when a caller never balances its `begin()`. */
  private static readonly watchdogTimeoutMs = 30_000;

  private readonly router = inject(Router);
  private readonly navigating = signal(false);
  private readonly pending = signal(0);
  private watchdogHandle?: ReturnType<typeof setTimeout>;

  /** True while a module transition is in progress. */
  public readonly loading = computed(() => this.navigating() || this.pending() > 0);

  constructor() {
    this.router.events.pipe(takeUntilDestroyed()).subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.navigating.set(true);
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError ||
        event instanceof NavigationSkipped
      ) {
        this.navigating.set(false);
      }
    });
  }

  /**
   * Registers one unit of loading work. Every call must be balanced with {@link end},
   * preferably from a `finally` block. Prefer {@link track} when the work is a promise.
   */
  public begin(): void {
    this.pending.update((count) => count + 1);
    this.armWatchdog();
  }

  /** Releases one unit of loading work registered with {@link begin}. */
  public end(): void {
    this.pending.update((count) => Math.max(0, count - 1));
    this.armWatchdog();
  }

  /**
   * Blocks the UI until `work` settles, and keeps the blocker released when it rejects.
   *
   * @example
   * await this.moduleLoading.track(this.reloadModuleInstance());
   */
  public async track<T>(work: PromiseLike<T>): Promise<T> {
    this.begin();
    try {
      return await work;
    } finally {
      this.end();
    }
  }

  private armWatchdog(): void {
    if (this.watchdogHandle != null) {
      clearTimeout(this.watchdogHandle);
      this.watchdogHandle = undefined;
    }

    if (this.pending() === 0) {
      return;
    }

    this.watchdogHandle = setTimeout(() => {
      this.watchdogHandle = undefined;
      console.warn(
        `[ModuleLoadingService] Loading work did not finish within ${ModuleLoadingService.watchdogTimeoutMs}ms, releasing the UI blocker.`
      );
      this.pending.set(0);
    }, ModuleLoadingService.watchdogTimeoutMs);
  }
}
