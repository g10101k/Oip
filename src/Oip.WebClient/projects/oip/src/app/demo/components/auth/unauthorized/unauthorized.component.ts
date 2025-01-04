import { Component, inject } from '@angular/core';
import { OidcSecurityService } from "angular-auth-oidc-client";

@Component({
    selector: 'unauthorized-error',
    templateUrl: './unauthorized.component.html',
})
export class UnauthorizedComponent {
  protected readonly oidcSecurityService = inject(OidcSecurityService);



}
