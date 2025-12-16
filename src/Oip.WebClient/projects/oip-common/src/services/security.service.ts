import { Injectable, OnDestroy, inject } from '@angular/core';
import {
  LoginResponse,
  LogoutAuthOptions,
  OidcSecurityService,
  PublicEventsService,
  EventTypes
} from 'angular-auth-oidc-client';
import { BehaviorSubject, Observable } from 'rxjs';
import { filter, map } from 'rxjs/operators';

/**
 * SecurityService extends OidcSecurityService to manage authentication,
 * token handling, and user role access in an Angular application.
 *
 * It provides helper methods for checking authentication, managing tokens,
 * determining user roles, and performing logout and refresh operations.
 */
@Injectable()
export class SecurityService extends OidcSecurityService implements OnDestroy {
  /**
   * Handles angular OIDC events.
   */
  private readonly publicEventsService = inject(PublicEventsService);

  /**
   * Stores the latest login response from checkAuth().
   */
  loginResponse = new BehaviorSubject<LoginResponse>(null);

  /**
   * Stores the decoded access token payload.
   */
  payload = new BehaviorSubject<any>(null);

  /**
   * Stores user-specific data from the login response.
   */
  userData: any;

  /**
   * Initializes service and subscribes to authentication events.
   * When a 'NewAuthenticationResult' event is received, the `auth` method is called.
   */
  constructor() {
    super();
    this.publicEventsService
      .registerForEvents()
      .pipe(filter((event) => event.type === EventTypes.NewAuthenticationResult))
      .subscribe(() => {
        this.auth();
      });
  }

  /**
   * Returns the ID token for the sign-in.
   * @returns A string with the id token.
   */
  override getAccessToken(): Observable<string> {
    return this.loginResponse.pipe(map((data) => data?.accessToken));
  }

  /**
   * Indicates whether the current user has the 'admin' role.
   *
   * @returns {boolean} True if the user is an admin, false otherwise.
   */
  get isAdmin(): boolean {
    return this.payload.getValue()?.realm_access?.roles?.includes('admin');
  }

  /**
   * Initiates authentication check and updates login response, user data,
   * and decoded token payload if authenticated.
   */
  auth() {
    super.checkAuth().subscribe((_response: LoginResponse) => {
      this.loginResponse.next(_response);
      this.userData = _response.userData;
      this.getPayloadFromAccessToken().subscribe((_token) => {
        this.payload.next(_token);
      });
    });
  }

  /**
   * Performs logout and clears the local token payload.
   *
   * @param {string} [configId] Optional configuration ID for logout.
   * @param {LogoutAuthOptions} [logoutAuthOptions] Optional logout options.
   */
  logout(configId?: string, logoutAuthOptions?: LogoutAuthOptions) {
    this.logoff(configId, logoutAuthOptions).subscribe((x) => this.payload.next(x));
  }

  /**
   * Completes the BehaviorSubjects when the service is destroyed to avoid memory leaks.
   */
  ngOnDestroy(): void {
    this.loginResponse.complete();
    this.payload.complete();
  }

  /**
   * Checks whether the current access token is expired based on the 'exp' claim.
   *
   * @returns {Observable<boolean>} Observable that emits true if the token is expired.
   */
  isTokenExpired(): Observable<boolean> {
    return this.getPayloadFromAccessToken().pipe(
      map((payload) => {
        return payload.exp < Math.floor(Date.now() / 1000);
      })
    );
  }
}
