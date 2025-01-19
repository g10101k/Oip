import { Injectable, OnDestroy, OnInit } from '@angular/core';
import { LoginResponse, LogoutAuthOptions, OidcSecurityService } from "angular-auth-oidc-client";
import { BehaviorSubject, Observable, Subject } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class SecurityService extends OidcSecurityService implements OnDestroy {

  loginResponse = new BehaviorSubject<LoginResponse>(null);
  loginResponse$ = this.loginResponse.asObservable();
  token = new BehaviorSubject<any>(null);
  token$ = this.token.asObservable();
  userData: any;
  get isAdmin(): any {
    return this.token.getValue()?.realm_access?.roles?.includes('admin');
  }

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

  logout(configId?: string, logoutAuthOptions?: LogoutAuthOptions) {
    this.logoff(configId, logoutAuthOptions).subscribe(x => this.token.next(x));
  }

  ngOnDestroy(): void {
    this.loginResponse.complete();
    this.token.complete();
  }
}

