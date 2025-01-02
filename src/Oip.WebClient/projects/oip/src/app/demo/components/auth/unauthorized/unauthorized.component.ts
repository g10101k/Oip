import { Component, inject } from '@angular/core';
import { OidcSecurityService } from "angular-auth-oidc-client";
import { ButtonDirective } from 'primeng/button';

@Component({
    selector: 'unauthorized-error',
    templateUrl: './unauthorized.component.html',
    imports: [ButtonDirective]
})
export class UnauthorizedComponent {
  protected readonly oidcSecurityService = inject(OidcSecurityService);
}
