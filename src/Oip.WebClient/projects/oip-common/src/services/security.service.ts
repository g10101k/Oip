import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy, inject } from '@angular/core';
import { BehaviorSubject, Observable, ReplaySubject, firstValueFrom, of } from 'rxjs';
import { catchError, distinctUntilChanged, filter, tap } from 'rxjs/operators';

type RefreshCustomParams = { [key: string]: string | number | boolean };

export type AuthSessionRefreshResult = {
  isAuthenticated: boolean;
  userData: any;
};

export type AuthCsrfToken = {
  token: string;
  headerName: string;
};

type AuthSessionResponse = {
  isAuthenticated: boolean;
  userName?: string;
  displayName?: string;
  email?: string;
  roles: string[];
};

type CurrentUser = {
  userName?: string;
  displayName?: string;
  email?: string;
  roles: string[];
  preferred_username?: string;
  name?: string;
  given_name?: string;
  family_name?: string;
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
    customParams?: RefreshCustomParams,
    configId?: string
  ): Observable<AuthSessionRefreshResult>;

  abstract getCsrfToken(): Observable<AuthCsrfToken | null>;

  abstract isAdmin(): boolean;

  abstract authorize(): void;

  abstract payload: BehaviorSubject<any>;
}

@Injectable()
export class BffSecurityService implements OnDestroy, SecurityService {
  private readonly http = inject(HttpClient);
  private readonly authenticated = new BehaviorSubject<boolean | null>(null);
  private readonly currentUser = new BehaviorSubject<any>(null);
  private readonly csrfToken = new ReplaySubject<AuthCsrfToken | null>(1);

  public readonly payload = new BehaviorSubject<any>(null);

  auth(): void {
    this.http.get<AuthSessionResponse>(this.buildUrl('api/security/get-current-auth-session'), {
      withCredentials: true
    }).pipe(
      tap((session) => this.applySession(session)),
      catchError(() => {
        this.applyAnonymousSession();
        return of(null);
      })
    ).subscribe();
  }

  logout(): void {
    firstValueFrom(this.getCsrfToken()).then((csrfToken) => {
      const form = this.createPostForm(this.buildUrl('api/security/delete-auth-session'));
      if (csrfToken?.token) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = csrfToken.token;
        form.appendChild(tokenInput);
      }
      document.body.appendChild(form);
      form.submit();
    });
  }

  isAuthenticated(): Observable<boolean> {
    return this.authenticated.asObservable().pipe(
      filter((value): value is boolean => value !== null),
      distinctUntilChanged()
    );
  }

  getAccessToken(): Observable<string> {
    return of('');
  }

  isTokenExpired(): Observable<boolean> {
    return of(false);
  }

  getCurrentUser(): any {
    return this.currentUser.getValue();
  }

  getCurrentUser$(): Observable<any> {
    return this.currentUser.asObservable();
  }

  forceRefreshSession(): Observable<AuthSessionRefreshResult> {
    this.auth();
    return of({
      isAuthenticated: this.authenticated.getValue() === true,
      userData: this.currentUser.getValue()
    });
  }

  getCsrfToken(): Observable<AuthCsrfToken | null> {
    this.http.get<AuthCsrfToken>(this.buildUrl('api/security/get-auth-csrf-token'), {
      withCredentials: true
    }).pipe(
      catchError(() => of(null))
    ).subscribe((token) => this.csrfToken.next(token));

    return this.csrfToken.asObservable();
  }

  isAdmin(): boolean {
    return this.payload.getValue()?.realm_access?.roles?.includes('admin') ?? false;
  }

  authorize(): void {
    const form = this.createPostForm(this.buildUrl('api/security/create-auth-session'));
    document.body.appendChild(form);
    form.submit();
  }

  ngOnDestroy(): void {
    this.authenticated.complete();
    this.currentUser.complete();
    this.payload.complete();
  }

  private applySession(session: AuthSessionResponse): void {
    const roles = session.roles ?? [];
    const user = this.createCurrentUser(session, roles);

    this.authenticated.next(session.isAuthenticated);
    this.currentUser.next(user);
    this.payload.next({realm_access: {roles}, ...user});
  }

  private createCurrentUser(session: AuthSessionResponse, roles: string[]): CurrentUser {
    const displayName = session.displayName || session.userName || session.email;
    const nameParts = this.splitDisplayName(displayName);

    return {
      userName: session.userName,
      displayName,
      email: session.email,
      roles,
      preferred_username: session.userName,
      name: displayName,
      given_name: nameParts.givenName,
      family_name: nameParts.familyName
    };
  }

  private splitDisplayName(displayName?: string): { givenName?: string; familyName?: string } {
    const parts = displayName?.trim().split(/\s+/).filter(Boolean) ?? [];

    return {
      givenName: parts[0],
      familyName: parts.length > 1 ? parts.slice(1).join(' ') : undefined
    };
  }

  private applyAnonymousSession(): void {
    this.authenticated.next(false);
    this.currentUser.next(null);
    this.payload.next(null);
    this.csrfToken.next(null);
  }

  private createPostForm(action: string): HTMLFormElement {
    const form = document.createElement('form');
    form.method = 'post';
    form.action = action;
    form.style.display = 'none';
    return form;
  }

  private buildUrl(path: string): string {
    const baseUrl = document.getElementsByTagName('base')[0].href;
    const normalizedBase = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
    return `${normalizedBase}${path}`;
  }
}
