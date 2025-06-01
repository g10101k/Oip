import { Injectable, OnDestroy } from '@angular/core';
import { LoginResponse, LogoutAuthOptions, OidcSecurityService } from "angular-auth-oidc-client";
import { BehaviorSubject, Observable } from "rxjs";
import { map } from "rxjs/operators";

/**
 * SecurityService extends OidcSecurityService to manage authentication,
 * token handling, and user role access in an Angular application.
 *
 * It provides helper methods for checking authentication, managing tokens,
 * determining user roles, and performing logout and refresh operations.
 */
@Injectable({ providedIn: 'root' })
export class SecurityService extends OidcSecurityService implements OnDestroy {
  /**
   * Stores the latest login response from checkAuth().
   */
  loginResponse = new BehaviorSubject<LoginResponse>(null);
  /**
   * Stores the decoded access token payload.
   */
  token = new BehaviorSubject<any>(null);
  /**
   * Stores user-specific data from the login response.
   */
  userData: any;

  /**
   * Indicates whether the current user has the 'admin' role.
   *
   * @returns {boolean} True if the user is an admin, false otherwise.
   */
  get isAdmin(): boolean {
    return this.token.getValue()?.realm_access?.roles?.includes('admin');
  }

  /**
   * Initiates authentication check and updates login response, user data,
   * and decoded token payload if authenticated.
   */
  auth() {
    super.checkAuth().subscribe((_response: LoginResponse) => {
      this.loginResponse.next(_response);
      this.userData = _response.userData;
      this.getPayloadFromAccessToken().subscribe(_token => {
          this.token.next(_token);
        }
      );
    });
  }

  /**
   * Performs logout and clears the local token payload.
   *
   * @param {string} [configId] Optional configuration ID for logout.
   * @param {LogoutAuthOptions} [logoutAuthOptions] Optional logout options.
   */
  logout(configId?: string, logoutAuthOptions?: LogoutAuthOptions) {
    this.logoff(configId, logoutAuthOptions).subscribe(x => this.token.next(x));
  }

  /**
   * Completes the BehaviorSubjects when the service is destroyed to avoid memory leaks.
   */
  ngOnDestroy(): void {
    this.loginResponse.complete();
    this.token.complete();
  }

  /**
   * Checks whether the current access token is expired based on the 'exp' claim.
   *
   * @returns {Observable<boolean>} Observable that emits true if the token is expired.
   */
  isTokenExpired(): Observable<boolean> {
    return this.getPayloadFromAccessToken().pipe(
      map((token) => {
        return token.exp < Math.floor(Date.now() / 1000);
      }));
  }
}
