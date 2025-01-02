import { inject, Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { OidcSecurityService } from "angular-auth-oidc-client";
import { AbstractAuthGuardService } from "../app.config";

@Injectable()
export class AuthGuardService implements AbstractAuthGuardService {
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly router = inject(Router);

  canActivate(): Observable<boolean | UrlTree> {
    return this.oidcSecurityService.isAuthenticated$.pipe(
      take(1),
      map((authenticatedResult) => {
        if (authenticatedResult.isAuthenticated) {
          return true;
        }
        return this.router.parseUrl('/unauthorized');
      })
    );
  }
}
