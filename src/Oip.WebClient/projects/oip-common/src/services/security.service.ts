import { Injectable, OnDestroy, inject } from '@angular/core';
import {
  LoginResponse,
  LogoutAuthOptions,
  OidcSecurityService,
  PublicEventsService,
  EventTypes,
  AuthOptions
} from 'angular-auth-oidc-client';
import { BehaviorSubject, firstValueFrom, finalize, from, merge, Observable, ReplaySubject, shareReplay } from 'rxjs';
import { distinctUntilChanged, filter, map } from 'rxjs/operators';

type RefreshCustomParams = { [key: string]: string | number | boolean };

type WebLockManager = {
  request<T>(
    name: string,
    options: { mode: 'exclusive' },
    callback: () => T | Promise<T>
  ): Promise<T>;
};

type RefreshLockInfo = {
  ownerId: string;
  expiresAt: number;
};

type RefreshResultInfo = {
  configId?: string;
  ownerId: string;
  status: 'success' | 'error';
  timestamp: number;
};

type WaitForRefreshResult = 'success' | 'error' | 'timeout';

export abstract class SecurityService {
  abstract auth(): void;

  abstract logout(): void;

  abstract isAuthenticated(): Observable<boolean>;

  abstract getAccessToken(): Observable<string>;

  abstract isTokenExpired(): Observable<boolean>;

  abstract getCurrentUser(): any;

  abstract getCurrentUser$(): Observable<any>;

  abstract forceRefreshSession(customParams?: RefreshCustomParams, configId?: string): Observable<LoginResponse>;

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
  private readonly refreshLockKeyPrefix = 'oip:keycloak-refresh-lock';
  private readonly refreshResultKeyPrefix = 'oip:keycloak-refresh-result';
  private readonly refreshLockTtlMs = 15000;
  private readonly refreshWaitTimeoutMs = 10000;
  private readonly refreshTabId = this.createRefreshTabId();
  private refreshSession$?: Observable<LoginResponse>;
  private readonly storageListener = (event: StorageEvent) => this.handleStorageEvent(event);

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
        super.getAccessToken().subscribe(token => {this.accessToken.next(token); });
        this.auth();
      });

    window.addEventListener('storage', this.storageListener);
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
    return merge(
      super.getAccessToken(configId),
      this.accessToken.asObservable()
    ).pipe(distinctUntilChanged());
  }

  override forceRefreshSession(customParams?: RefreshCustomParams, configId?: string): Observable<LoginResponse> {
    if (!this.refreshSession$) {
      this.refreshSession$ = from(this.runSynchronizedRefresh(customParams, configId)).pipe(
        finalize(() => {
          this.refreshSession$ = undefined;
        }),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }

    return this.refreshSession$;
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
      this.loginResponse.next(_response);
      this.currentUser.next(_response.userData);
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
    window.removeEventListener('storage', this.storageListener);
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

  private async runSynchronizedRefresh(
    customParams?: RefreshCustomParams,
    configId?: string
  ): Promise<LoginResponse> {
    const webLocks = this.getWebLocks();
    if (webLocks) {
      return webLocks.request(this.getRefreshLockKey(configId), { mode: 'exclusive' }, async () => {
        const currentState = await this.syncAuthState(configId);
        if (!(await this.isCurrentAccessTokenExpired())) {
          return currentState;
        }

        return this.refreshAsLockOwner(customParams, configId);
      });
    }

    return this.runSynchronizedRefreshWithStorageLock(customParams, configId);
  }

  private async runSynchronizedRefreshWithStorageLock(
    customParams?: RefreshCustomParams,
    configId?: string
  ): Promise<LoginResponse> {
    const startedAt = Date.now();

    if (this.tryAcquireRefreshLock(configId)) {
      try {
        const currentState = await this.syncAuthState(configId);
        if (!(await this.isCurrentAccessTokenExpired())) {
          return currentState;
        }

        return await this.refreshAsLockOwner(customParams, configId);
      } finally {
        this.releaseRefreshLock(configId);
      }
    }

    const waitResult = await this.waitForRefreshResult(startedAt, configId);
    if (waitResult === 'success') {
      return this.syncAuthState(configId);
    }

    if (waitResult === 'error') {
      throw new Error('Token refresh failed in another tab.');
    }

    return this.runSynchronizedRefreshWithStorageLock(customParams, configId);
  }

  private async refreshAsLockOwner(
    customParams?: RefreshCustomParams,
    configId?: string
  ): Promise<LoginResponse> {
    try {
      const response = await firstValueFrom(super.forceRefreshSession(customParams, configId));
      await this.applyLoginResponse(response, configId);
      this.publishRefreshResult('success', configId);
      return response;
    } catch (error) {
      this.publishRefreshResult('error', configId);
      throw error;
    }
  }

  private async syncAuthState(configId?: string): Promise<LoginResponse> {
    const response = await firstValueFrom(super.checkAuth(undefined, configId));
    await this.applyLoginResponse(response, configId);
    return response;
  }

  private async applyLoginResponse(response: LoginResponse, configId?: string): Promise<void> {
    this.loginResponse.next(response);
    this.currentUser.next(response.userData);

    const token = await firstValueFrom(super.getAccessToken(configId));
    this.accessToken.next(token);

    const payload = await firstValueFrom(this.getPayloadFromAccessToken());
    this.payload.next(payload);
  }

  private async isCurrentAccessTokenExpired(): Promise<boolean> {
    const payload = await firstValueFrom(this.getPayloadFromAccessToken());
    return !payload?.exp || payload.exp < Math.floor(Date.now() / 1000);
  }

  private tryAcquireRefreshLock(configId?: string): boolean {
    const lockKey = this.getRefreshLockKey(configId);
    const now = Date.now();
    const currentLock = this.readRefreshLock(lockKey);

    if (currentLock && currentLock.ownerId !== this.refreshTabId && currentLock.expiresAt > now) {
      return false;
    }

    const nextLock: RefreshLockInfo = {
      ownerId: this.refreshTabId,
      expiresAt: now + this.refreshLockTtlMs
    };

    localStorage.setItem(lockKey, JSON.stringify(nextLock));

    return this.readRefreshLock(lockKey)?.ownerId === this.refreshTabId;
  }

  private releaseRefreshLock(configId?: string): void {
    const lockKey = this.getRefreshLockKey(configId);
    const currentLock = this.readRefreshLock(lockKey);

    if (currentLock?.ownerId === this.refreshTabId) {
      localStorage.removeItem(lockKey);
    }
  }

  private waitForRefreshResult(startedAt: number, configId?: string): Promise<WaitForRefreshResult> {
    const resultKey = this.getRefreshResultKey(configId);

    return new Promise((resolve) => {
      const timeoutId = window.setTimeout(() => {
        cleanup();
        resolve('timeout');
      }, this.refreshWaitTimeoutMs);

      const intervalId = window.setInterval(() => {
        const result = this.tryResolveRefreshResult(resultKey, startedAt, configId);
        if (result) {
          cleanup();
          resolve(result);
        }
      }, 250);

      const onStorage = (event: StorageEvent) => {
        if (event.key === resultKey) {
          const result = this.tryResolveRefreshResult(resultKey, startedAt, configId);
          if (result) {
            cleanup();
            resolve(result);
          }
        }
      };

      const cleanup = () => {
        window.clearTimeout(timeoutId);
        window.clearInterval(intervalId);
        window.removeEventListener('storage', onStorage);
      };

      window.addEventListener('storage', onStorage);

      const result = this.tryResolveRefreshResult(resultKey, startedAt, configId);
      if (result) {
        cleanup();
        resolve(result);
      }
    });
  }

  private tryResolveRefreshResult(
    resultKey: string,
    startedAt: number,
    configId: string | undefined
  ): Exclude<WaitForRefreshResult, 'timeout'> | null {
    const result = this.readRefreshResult(resultKey);

    if (!result || result.timestamp < startedAt || result.configId !== configId) {
      return null;
    }

    return result.status;
  }

  private publishRefreshResult(status: RefreshResultInfo['status'], configId?: string): void {
    const result: RefreshResultInfo = {
      configId,
      ownerId: this.refreshTabId,
      status,
      timestamp: Date.now()
    };

    localStorage.setItem(this.getRefreshResultKey(configId), JSON.stringify(result));
  }

  private handleStorageEvent(event: StorageEvent): void {
    if (!event.key?.startsWith(`${this.refreshResultKeyPrefix}:`) || !event.newValue) {
      return;
    }

    const result = this.parseRefreshResult(event.newValue);
    if (!result || result.ownerId === this.refreshTabId || result.status !== 'success') {
      return;
    }

    void this.syncAuthState(result.configId).catch(() => {
      // Another auth flow will handle failures; the important bit here is not keeping a stale in-memory token.
    });
  }

  private readRefreshLock(lockKey: string): RefreshLockInfo | null {
    try {
      const value = localStorage.getItem(lockKey);
      return value ? JSON.parse(value) as RefreshLockInfo : null;
    } catch {
      return null;
    }
  }

  private readRefreshResult(resultKey: string): RefreshResultInfo | null {
    try {
      const value = localStorage.getItem(resultKey);
      return value ? this.parseRefreshResult(value) : null;
    } catch {
      return null;
    }
  }

  private parseRefreshResult(value: string): RefreshResultInfo | null {
    try {
      return JSON.parse(value) as RefreshResultInfo;
    } catch {
      return null;
    }
  }

  private getRefreshLockKey(configId?: string): string {
    return `${this.refreshLockKeyPrefix}:${configId ?? 'default'}`;
  }

  private getRefreshResultKey(configId?: string): string {
    return `${this.refreshResultKeyPrefix}:${configId ?? 'default'}`;
  }

  private getWebLocks(): WebLockManager | null {
    return (navigator as Navigator & { locks?: WebLockManager }).locks ?? null;
  }

  private createRefreshTabId(): string {
    return window.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`;
  }
}
