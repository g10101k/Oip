import { inject, Injectable } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { OidcSecurityService } from "angular-auth-oidc-client";

@Injectable()
export class AuthGuardService {
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
