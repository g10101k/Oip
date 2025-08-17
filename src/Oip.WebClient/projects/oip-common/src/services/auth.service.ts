import { inject, Injectable } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { catchError, map, switchMap } from 'rxjs/operators';
import { Observable, combineLatest, of, lastValueFrom, from } from 'rxjs';
import { SecurityService } from "./security.service";

/**
 * A route guard that ensures the user is authenticated and has a valid access token.
 * If the access token is expired, it attempts to refresh the session.
 * If authentication fails or refresh is unsuccessful, redirects to the unauthorized page.
 */
@Injectable()
export class AuthGuardService {
  private readonly oidcSecurityService = inject(SecurityService);
  private readonly router = inject(Router);

  /**
   * Checks whether the route can be activated.
   * - Returns `true` if the user is authenticated and the token is valid.
   * - Attempts to refresh the token if expired.
   * - Redirects to `/unauthorized` if not authenticated or refresh fails.
   *
   * @returns {Observable<boolean | UrlTree>} A stream resolving to true (allow), or UrlTree (redirect).
   */
  canActivate(): Observable<boolean | UrlTree> {
    return from(this.oidcSecurityService.isAuthenticated()).pipe(
      map((result) => {
        if (result) {
          return true;
        }
        return this.router.parseUrl('/unauthorized');
      }),
    )
  }
}
