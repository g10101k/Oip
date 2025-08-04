import { Injectable, OnDestroy, inject } from '@angular/core';
import {
  LoginResponse,
  LogoutAuthOptions,
  OidcSecurityService,
  PublicEventsService,
  EventTypes
} from "angular-auth-oidc-client";
import { BehaviorSubject, Observable, Subject } from "rxjs";
import { filter, map } from "rxjs/operators";

/**
 * SecurityService extends OidcSecurityService to manage authentication,
 * token handling, and user role access in an Angular application.
 *
 * It provides helper methods for checking authentication, managing tokens,
 * determining user roles, and performing logout and refresh operations.
 */
@Injectable({ providedIn: "root" })
export class SecurityService extends OidcSecurityService implements OnDestroy {
  /**
   * Handles angular OIDC events.
   */
  private readonly publicEventsService = inject(PublicEventsService);

  /**
   * Stores the decoded access token payload.
   */
  public payload = new BehaviorSubject<any>(null);

  /**
   * Stores user-specific data from the login response.
   */
  public user = new Subject<any>();

  public accessToken: string | null = null;

  public onLogin = new Subject<boolean>();

  /**
   * Initializes service and subscribes to authentication events.
   * When a 'NewAuthenticationResult' event is received, the `auth` method is called.
   */
  constructor() {
    super();
    this.publicEventsService.registerForEvents()
      .pipe(filter(event => event.type === EventTypes.NewAuthenticationResult))
      .subscribe((event) => {
        console.log(event)

        this.getAuthenticationResult().subscribe((s) => {
          console.log("getAuthenticationResult");
          console.log(s.access_token)
          this.accessToken = s.access_token;

          this.onLogin.next(true)
        })
      });
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
      console.log('checkAuth()')
      console.log(_response)
      if (!_response.isAuthenticated){
        this.forceRefreshSession().subscribe(x=>{
          console.log('checkAuth()')
          console.log(x)
        })
        return;
      }


      this.isTokenExpired().subscribe((isExpired) => {
        if (isExpired) {
          console.log("auth.checkAuth.isExpired - true")

          console.log(_response)
        } else {
          console.log("auth.checkAuth.isExpired - false")

          console.log(_response)

          this.processLoginResponse(_response);
          this.onLogin.next(true)
          this.getPayloadFromAccessToken().subscribe(payload => {
              console.log("getPayloadFromAccessToken")
              this.payload.next(payload);
            }
          );
        }
      })
    });
  }

  private processLoginResponse(_response: LoginResponse) {
    this.accessToken = _response.accessToken;
    this.user.next(_response.userData);
  }

  /**
   * Performs logout and clears the local token payload.
   *
   * @param {string} [configId] Optional configuration ID for logout.
   * @param {LogoutAuthOptions} [logoutAuthOptions] Optional logout options.
   */
  logout(configId?: string, logoutAuthOptions?: LogoutAuthOptions) {
    this.logoff(configId, logoutAuthOptions).subscribe(x => this.payload.next(x));
  }

  /**
   * Completes the BehaviorSubjects when the service is destroyed to avoid memory leaks.
   */
  ngOnDestroy(): void {
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
      }));
  }
}
