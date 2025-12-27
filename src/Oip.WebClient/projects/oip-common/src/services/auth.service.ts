import { inject, Injectable } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { catchError, map, switchMap } from 'rxjs/operators';
import { Observable, combineLatest, of } from 'rxjs';
import { SecurityService } from './security.service';

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
    return combineLatest([this.oidcSecurityService.isAuthenticated(), this.oidcSecurityService.isTokenExpired()]).pipe(
      switchMap(([authenticated, tokenExpired]) => {
        if (!authenticated) {
          return of(this.router.parseUrl('/unauthorized'));
        }
        if (!tokenExpired) {
          return of(true);
        }

        // Token is expired; attempt to refresh
        return this.tryRefreshToken();
      })
    );
  }

  /**
   * Attempts to refresh the session using the refresh token.
   * If successful, allows route activation; otherwise, redirects to `/unauthorized`.
   *
   * @returns {boolean | UrlTree} A stream resolving to true or redirect UrlTree.
   */
  tryRefreshToken(): Observable<boolean | UrlTree> {
    return this.oidcSecurityService.forceRefreshSession().pipe(
      map((refreshSuccess) => {
        return refreshSuccess ? true : this.router.parseUrl('/unauthorized');
      }),
      catchError((err) => {
        console.warn(err);
        return of(this.router.parseUrl('/unauthorized'));
      })
    );
  }
}
