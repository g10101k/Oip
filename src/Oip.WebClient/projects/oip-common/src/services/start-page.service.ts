import { inject, Injectable } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { ModuleInstanceDto } from '../api/data-contracts';
import { MenuApi } from '../api/menu.api';
import { SecurityService } from './security.service';

/**
 * Resolves the module instance the current user lands on when no explicit route is requested.
 *
 * The menu returned by the backend is already filtered by the rights of the current user, so the
 * first navigable leaf of that menu is always a module the user may open. When the user picked a
 * start module explicitly it comes back marked with `isStart`, and a module that was deleted or
 * whose rights were revoked simply disappears from the menu, which falls back to the first leaf.
 *
 * The resolved url is cached per user and dropped whenever the authenticated user changes or the
 * start module is reassigned.
 */
@Injectable({ providedIn: 'root' })
export class StartPageService {
  private readonly menuApi = inject(MenuApi);
  private readonly router = inject(Router);
  private readonly securityService = inject(SecurityService);
  private request: Promise<UrlTree | null> | undefined = undefined;
  private cachedIdentity: string | undefined = undefined;

  constructor() {
    this.securityService.payload.subscribe((payload) => {
      // The menu depends on both the user and the roles of the current session.
      const roles: string[] = payload?.realm_access?.roles ?? [];
      const identity = `${payload?.preferred_username ?? ''}|${[...roles].sort().join(',')}`;
      if (identity !== this.cachedIdentity) {
        this.cachedIdentity = identity;
        this.clearCache();
      }
    });
  }

  /**
   * Gets the url of the start module instance.
   *
   * @returns The url tree to navigate to, or `null` when the user has no module available.
   */
  public resolveStartUrl(): Promise<UrlTree | null> {
    if (this.request) {
      return this.request;
    }

    this.request = this.loadStartUrl().catch((error) => {
      this.request = undefined;
      throw error;
    });

    return this.request;
  }

  /**
   * Makes the module instance the start page of the current user.
   *
   * @param moduleInstanceId Module instance to open by default.
   */
  public async setStartModule(moduleInstanceId: number): Promise<void> {
    await this.menuApi.setStartModule({ id: moduleInstanceId });
    this.clearCache();
  }

  /**
   * Clears the start page of the current user, falling back to the first available module.
   */
  public async clearStartModule(): Promise<void> {
    await this.menuApi.deleteStartModule();
    this.clearCache();
  }

  /**
   * Drops the cached url, for example after the menu was changed.
   */
  public clearCache(): void {
    this.request = undefined;
  }

  private async loadStartUrl(): Promise<UrlTree | null> {
    const menu = (await this.menuApi.get()) ?? [];
    const item = this.findStartItem(menu);

    return item ? this.router.createUrlTree(item.routerLink!) : null;
  }

  private findStartItem(menu: ModuleInstanceDto[]): ModuleInstanceDto | null {
    const leaves = this.collectNavigableLeaves(menu);
    return leaves.find((item) => item.isStart) ?? leaves[0] ?? null;
  }

  private collectNavigableLeaves(items: ModuleInstanceDto[]): ModuleInstanceDto[] {
    const result: ModuleInstanceDto[] = [];

    for (const item of [...items].sort((a, b) => (a.order ?? 0) - (b.order ?? 0))) {
      if (item.separator) {
        continue;
      }

      if (item.items?.length) {
        // A folder is never a landing page itself, its children are.
        result.push(...this.collectNavigableLeaves(item.items));
        continue;
      }

      // A module without a real route would resolve back to the empty path and loop.
      if (item.routerLink?.some((segment) => !!segment && segment !== '/')) {
        result.push(item);
      }
    }

    return result;
  }
}
