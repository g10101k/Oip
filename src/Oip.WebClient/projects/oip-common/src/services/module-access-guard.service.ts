import { inject } from '@angular/core';
import { CanActivateChildFn, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { filter, take } from 'rxjs/operators';
import { ModuleInstanceRightsService } from './module-instance-rights.service';
import { SecurityService } from './security.service';

/**
 * Route data flag marking a route that only administrators may open.
 */
export interface ModuleAccessRouteData {
  requireAdmin?: boolean;
}

/**
 * Guards module routes against direct navigation without the required rights.
 *
 * - Routes flagged with `data: { requireAdmin: true }` require the `admin` role.
 * - Routes carrying an `:id` segment require the `read` right on that module instance.
 * - Everything else is left to {@link AuthGuardService}.
 *
 * Denied navigation is redirected to the access denied page. This is a UX guard only:
 * the backend checks the same rights on every module endpoint.
 */
export const moduleAccessGuard: CanActivateChildFn = async (route) => {
  const router = inject(Router);
  const securityService = inject(SecurityService);
  const rightsService = inject(ModuleInstanceRightsService);
  const accessDenied = router.parseUrl('/access');

  if ((route.data as ModuleAccessRouteData)?.requireAdmin) {
    // Wait for the session payload so a direct URL entry does not read roles before they are loaded.
    await firstValueFrom(
      securityService.payload.pipe(
        filter((payload) => payload != null),
        take(1)
      )
    );
    return securityService.isAdmin() ? true : accessDenied;
  }

  const routeId = route.paramMap.get('id');
  if (routeId == null) {
    return true;
  }

  const moduleInstanceId = Number(routeId);
  if (!Number.isFinite(moduleInstanceId)) {
    return accessDenied;
  }

  try {
    return (await rightsService.canRead(moduleInstanceId)) ? true : accessDenied;
  } catch (error) {
    console.error('Failed to load module instance rights', error);
    return accessDenied;
  }
};
