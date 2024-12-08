import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Navigation, Router, RouterStateSnapshot } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { BaseService } from "./base.service";
import { ConfigService } from './config.service';
import { environment } from "../environments/environment";

export interface IUser {
  name: string;
  avatarUrl?: string
}

const defaultPath = '/';
const defaultUser = {
  name: 'Olis Kaplan',
  avatarUrl: 'https://js.devexpress.com/Demos/WidgetsGallery/JSDemos/images/employees/06.png'
};

@Injectable()
export class AuthService extends BaseService {
  private _user: IUser | null = defaultUser;
  // Observable navItem source
  private _authNavStatusSource = new BehaviorSubject<boolean>(false);
  // Observable navItem stream
  authNavStatus$ = this._authNavStatusSource.asObservable();

  private lastNavigation: Navigation;

  constructor(private http: HttpClient, private configService: ConfigService, private router: Router) {
    super();


  }

  get loggedIn(): boolean {

    return true;
  }


  set lastAuthenticatedPath(value: string) {
    localStorage.setItem('lastAuthenticatedPath', value);
  }

  get lastAuthenticatedPath(): string
  {
    return localStorage.getItem('lastAuthenticatedPath');
  }


  get name(): string {

      return '';
  }


  async getUser() {

    return {
      isOk: false,
      data: defaultUser
    };
  }

  async createAccount(email: string, password: string) {
    try {
      // Send request
      console.log(email, password);

      await this.router.navigate(['/create-account']);
      return {
        isOk: true
      };
    } catch {
      return {
        isOk: false,
        message: "Failed to create account"
      };
    }
  }

  async changePassword(email: string, recoveryCode: string) {
    try {
      // Send request
      console.log(email, recoveryCode);

      return {
        isOk: true
      };
    } catch {
      return {
        isOk: false,
        message: "Failed to change password"
      }
    }
  }

  async resetPassword(email: string) {
    try {
      // Send request
      console.log(email);

      return {
        isOk: true
      };
    } catch {
      return {
        isOk: false,
        message: "Failed to reset password"
      };
    }
  }

  async logOut() {

  }

  login() {
    this.lastNavigation = this.router.getCurrentNavigation();

    if (environment.useAuthentication) {
    }
    return;
  }

  async completeAuthentication() {
    this._authNavStatusSource.next(this.isAuthenticated());
    await this.router.navigate([this.lastAuthenticatedPath]);
  }

  register(userRegistration: any) {
    return this.http.post(this.configService.authApiURI + '/account', userRegistration).pipe(catchError(this.handleError));
  }

  isAuthenticated(): boolean {
    if (environment.useAuthentication) {
    }
    return true;
  }

  async signout() {
  }


  async navigateToPrevious() {
    this.router.navigate([this.lastNavigation]);
  }
}

@Injectable()
export class AuthGuardService implements CanActivate {
  constructor(private router: Router, private authService: AuthService) {
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (environment.useAuthentication) {
      if (this.authService.isAuthenticated()) {
        return true;
      }
      const isLoggedIn = this.authService.loggedIn;
      const isAuthForm = [
        'login-form',
        'reset-password',
        'create-account',
        'auth-callback',
        'change-password/:recoveryCode'
      ].includes(route.routeConfig?.path || defaultPath);

      if (isLoggedIn && isAuthForm) {
        this.authService.lastAuthenticatedPath = defaultPath;
        this.router.navigate([defaultPath]);
        return false;
      }

      if (!isLoggedIn && !isAuthForm) {
        this.authService.lastAuthenticatedPath = route.routeConfig?.path;
        this.authService.login();
      }

      if (isLoggedIn) {
        this.authService.lastAuthenticatedPath = route.routeConfig?.path || defaultPath;
      }

      return isLoggedIn || isAuthForm;
    }
    return true;
  }
}
