import { Injectable, OnDestroy, inject } from '@angular/core';
import {
  LoginResponse,
  LogoutAuthOptions,
  OidcSecurityService,
  PublicEventsService,
  EventTypes,
  AuthOptions
} from 'angular-auth-oidc-client';
import { BehaviorSubject, merge, Observable, ReplaySubject, throwError } from 'rxjs';
import { catchError, distinctUntilChanged, filter, finalize, map, shareReplay, switchMap, tap } from 'rxjs/operators';

type RefreshLock = {
  owner: string;
  expiresAt: number;
};

type RefreshMessage = {
  owner: string;
  success: boolean;
  timestamp: number;
};

export abstract class SecurityService {
  abstract auth(): void;

  abstract logout(): void;

  abstract isAuthenticated(): Observable<boolean>;

  abstract getAccessToken(): Observable<string>;

  abstract isTokenExpired(): Observable<boolean>;

  abstract getCurrentUser(): any;

  abstract getCurrentUser$(): Observable<any>;

  abstract forceRefreshSession(
    customParams?: { [key: string]: string | number | boolean },
    configId?: string
  ): Observable<LoginResponse>;

  abstract isAdmin(): boolean;

  abstract authorize(configId?: string, authOptions?: AuthOptions): void;

  abstract payload: BehaviorSubject<any>;
}

/**
 * SecurityService extends OidcSecurityService to manage authentication,
 * token handling, and user role access in an Angular application.
 *
 * It provides helper methods for checking authentication, managing tokens,
 * determining user roles, and performing logout and refresh operations.
 */
@Injectable()
export class KeycloakSecurityService extends OidcSecurityService implements OnDestroy, SecurityService {
  private static readonly refreshLockKey = 'oip.auth.refresh.lock';

  private static readonly refreshResultKey = 'oip.auth.refresh.result';

  private static readonly refreshChannelName = 'oip.auth.refresh';

  private static readonly refreshLockTtlMs = 15000;

  private static readonly refreshWaitTimeoutMs = 20000;

  private readonly tabId = this.createTabId();

  private readonly refreshChannel = this.createRefreshChannel();

  private currentTabRefresh$: Observable<LoginResponse> | null = null;

  /**
   * Handles angular OIDC events.
   */
  private readonly publicEventsService = inject(PublicEventsService);

  /**
   * Stores the latest login response from checkAuth().
   */
  public loginResponse = new BehaviorSubject<LoginResponse>(null);

  /**
   * Stores the decoded access token payload.
   */
  public readonly payload = new BehaviorSubject<any>(null);

  /**
   * Stores user-specific data from the login response.
   */
  private readonly currentUser = new BehaviorSubject<any>(null);

  /**
   * Emits access token updates from initial auth check, manual refresh,
   * and library authentication events.
   */
  private accessToken = new ReplaySubject<string>(1);

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
        super.getAccessToken().subscribe((token) => {
          this.accessToken.next(token);
        });
        this.auth();
      });
  }

  getCurrentUser() {
    return this.currentUser.getValue();
  }

  getCurrentUser$(): Observable<any> {
    return this.currentUser.asObservable();
  }

  /**
   * Returns the ID token for the sign-in.
   * @returns A string with the id token.
   */
  override getAccessToken(configId?: string): Observable<string> {
    return merge(super.getAccessToken(configId), this.accessToken.asObservable()).pipe(distinctUntilChanged());
  }

  /**
   * Refreshes tokens in only one browser tab at a time. Other tabs wait for the
   * refresh result and then read the updated tokens from shared storage.
   */
  override forceRefreshSession(
    customParams?: { [key: string]: string | number | boolean },
    configId?: string
  ): Observable<LoginResponse> {
    if (this.currentTabRefresh$) {
      return this.currentTabRefresh$;
    }

    const refreshRequestedAt = Date.now();

    if (!this.canUseBrowserStorage()) {
      return this.refreshCurrentTab(customParams, configId);
    }

    if (this.tryAcquireRefreshLock()) {
      this.currentTabRefresh$ = this.refreshCurrentTab(customParams, configId).pipe(
        tap(() => this.publishRefreshResult(true)),
        catchError((error) => {
          this.publishRefreshResult(false);
          return throwError(() => error);
        }),
        finalize(() => {
          this.releaseRefreshLock();
          this.currentTabRefresh$ = null;
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );

      return this.currentTabRefresh$;
    }

    const refreshOwner = this.readRefreshLock()?.owner ?? null;

    return this.waitForRefreshResult(refreshRequestedAt, refreshOwner).pipe(
      switchMap((success) => {
        if (!success) {
          return throwError(() => new Error('Token refresh failed in another tab'));
        }

        return super.checkAuth(undefined, configId).pipe(tap((response) => this.setLoginResponse(response)));
      })
    );
  }

  /**
   * Indicates whether the current user has the 'admin' role.
   *
   * @returns {boolean} True if the user is an admin, false otherwise.
   */
  isAdmin(): boolean {
    return this.payload.getValue()?.realm_access?.roles?.includes('admin');
  }

  /**
   * Initiates authentication check and updates login response, user data,
   * and decoded token payload if authenticated.
   */
  auth() {
    super.checkAuth().subscribe((_response: LoginResponse) => {
      this.setLoginResponse(_response);
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
    this.refreshChannel?.close();
    this.loginResponse.complete();
    this.payload.complete();
    this.currentUser.complete();
    this.accessToken.complete();
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

  private refreshCurrentTab(
    customParams?: { [key: string]: string | number | boolean },
    configId?: string
  ): Observable<LoginResponse> {
    return super.forceRefreshSession(customParams, configId).pipe(tap((response) => this.setLoginResponse(response)));
  }

  private setLoginResponse(response: LoginResponse): void {
    this.loginResponse.next(response);
    this.currentUser.next(response?.userData);

    if (response?.accessToken) {
      this.accessToken.next(response.accessToken);
      this.getPayloadFromAccessToken().subscribe((_token) => {
        this.payload.next(_token);
      });
    }
  }

  private tryAcquireRefreshLock(): boolean {
    const now = Date.now();
    const currentLock = this.readRefreshLock();

    if (currentLock && currentLock.expiresAt > now && currentLock.owner !== this.tabId) {
      return false;
    }

    const lock: RefreshLock = {
      owner: this.tabId,
      expiresAt: now + KeycloakSecurityService.refreshLockTtlMs
    };

    localStorage.setItem(KeycloakSecurityService.refreshLockKey, JSON.stringify(lock));

    return this.readRefreshLock()?.owner === this.tabId;
  }

  private releaseRefreshLock(): void {
    if (this.readRefreshLock()?.owner === this.tabId) {
      localStorage.removeItem(KeycloakSecurityService.refreshLockKey);
    }
  }

  private readRefreshLock(): RefreshLock | null {
    const lock = localStorage.getItem(KeycloakSecurityService.refreshLockKey);

    if (!lock) {
      return null;
    }

    try {
      return JSON.parse(lock) as RefreshLock;
    } catch {
      localStorage.removeItem(KeycloakSecurityService.refreshLockKey);
      return null;
    }
  }

  private publishRefreshResult(success: boolean): void {
    const message: RefreshMessage = {
      owner: this.tabId,
      success,
      timestamp: Date.now()
    };

    localStorage.setItem(KeycloakSecurityService.refreshResultKey, JSON.stringify(message));
    this.refreshChannel?.postMessage(message);
  }

  private waitForRefreshResult(refreshRequestedAt: number, refreshOwner: string | null): Observable<boolean> {
    return new Observable<boolean>((subscriber) => {
      const completeWith = (success: boolean) => {
        if (!subscriber.closed) {
          subscriber.next(success);
          subscriber.complete();
        }
      };

      const handleMessage = (message: RefreshMessage | null) => {
        if (
          !message ||
          message.owner === this.tabId ||
          message.timestamp < refreshRequestedAt ||
          (refreshOwner && message.owner !== refreshOwner)
        ) {
          return;
        }

        completeWith(message.success);
      };

      const onBroadcastMessage = (event: MessageEvent<RefreshMessage>) => handleMessage(event.data);
      const onStorageMessage = (event: StorageEvent) => {
        if (event.key !== KeycloakSecurityService.refreshResultKey || !event.newValue) {
          return;
        }

        handleMessage(this.parseRefreshMessage(event.newValue));
      };

      this.refreshChannel?.addEventListener('message', onBroadcastMessage);
      window.addEventListener('storage', onStorageMessage);
      handleMessage(this.parseRefreshMessage(localStorage.getItem(KeycloakSecurityService.refreshResultKey)));

      const timeoutId = window.setTimeout(() => completeWith(false), KeycloakSecurityService.refreshWaitTimeoutMs);

      return () => {
        this.refreshChannel?.removeEventListener('message', onBroadcastMessage);
        window.removeEventListener('storage', onStorageMessage);
        window.clearTimeout(timeoutId);
      };
    });
  }

  private parseRefreshMessage(value: string | null): RefreshMessage | null {
    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as RefreshMessage;
    } catch {
      return null;
    }
  }

  private createTabId(): string {
    return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`;
  }

  private createRefreshChannel(): BroadcastChannel | null {
    return typeof BroadcastChannel === 'undefined'
      ? null
      : new BroadcastChannel(KeycloakSecurityService.refreshChannelName);
  }

  private canUseBrowserStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
