import { inject, Injectable } from '@angular/core';
import { MenuApi } from '../api/menu.api';
import { SecurityService } from './security.service';

/**
 * Reads the rights the current user has on a module instance.
 *
 * Results are cached per instance and dropped whenever the authenticated user changes.
 */
@Injectable()
export class ModuleInstanceRightsService {
  public static readonly readRight = 'read';

  private readonly menuApi = inject(MenuApi);
  private readonly securityService = inject(SecurityService);
  private readonly cache = new Map<number, Promise<string[]>>();
  private cachedIdentity: string | undefined = undefined;

  constructor() {
    this.securityService.payload.subscribe((payload) => {
      // Rights depend on both the user and the roles of the current session.
      const roles: string[] = payload?.realm_access?.roles ?? [];
      const identity = `${payload?.preferred_username ?? ''}|${[...roles].sort().join(',')}`;
      if (identity !== this.cachedIdentity) {
        this.cachedIdentity = identity;
        this.clearCache();
      }
    });
  }

  /**
   * Gets the rights of the current user on the given module instance.
   */
  public getRights(moduleInstanceId: number): Promise<string[]> {
    const cached = this.cache.get(moduleInstanceId);
    if (cached) {
      return cached;
    }

    const request = this.menuApi
      .getModuleInstanceRights({ id: moduleInstanceId })
      .then((rights) => rights ?? [])
      .catch((error) => {
        this.cache.delete(moduleInstanceId);
        throw error;
      });

    this.cache.set(moduleInstanceId, request);
    return request;
  }

  /**
   * Checks whether the current user has the `read` right on the given module instance.
   */
  public async canRead(moduleInstanceId: number): Promise<boolean> {
    const rights = await this.getRights(moduleInstanceId);
    return rights.includes(ModuleInstanceRightsService.readRight);
  }

  /**
   * Drops every cached result, for example after module security was changed.
   */
  public clearCache(): void {
    this.cache.clear();
  }
}
