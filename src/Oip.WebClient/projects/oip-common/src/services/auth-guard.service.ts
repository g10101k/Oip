import { inject, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { SecurityService } from './security.service';

/**
 * A route guard that ensures the user is authenticated and has a valid access token.
 * If the access token is expired, it attempts to refresh the session.
 * If authentication fails or refresh is unsuccessful, redirects to the unauthorized page.
 */
@Injectable()
export class AuthGuardService {
  private readonly oidcSecurityService = inject(SecurityService);

  /**
   * Checks whether the route can be activated.
   * - Returns `true` if the user is authenticated and the token is valid.
   * - Attempts to refresh the token if expired.
   * - Starts the authorization flow if not authenticated or refresh fails.
   *
   * @returns {Observable<boolean>} A stream resolving to true (allow), or false after starting authorization.
   */
  canActivate(returnUrl = '/'): Observable<boolean> {
    this.oidcSecurityService.auth();
    return this.oidcSecurityService.isAuthenticated().pipe(
      map((authenticated) => {
        if (authenticated) {
          return true;
        }

        this.oidcSecurityService.authorize(returnUrl);
        return false;
      })
    );
  }
}
