import { Component, inject } from '@angular/core';
import { LogoComponent, SecurityService } from "oip-common";
import { ButtonModule } from "primeng/button";
import { RippleModule } from "primeng/ripple";

@Component({
  selector: 'unauthorized-error',
  templateUrl: './unauthorized.component.html',
  standalone: true,
  imports: [
    LogoComponent,
    ButtonModule,
    RippleModule
  ]
})
export class UnauthorizedComponent {
  protected readonly securityService = inject(SecurityService);
}
