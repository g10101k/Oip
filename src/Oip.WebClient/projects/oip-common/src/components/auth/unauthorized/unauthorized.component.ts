import { Component, inject } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { RippleModule } from "primeng/ripple";
import { LogoComponent } from "../../logo.component";
import { SecurityService } from "../../../services/security.service";

@Component({
  selector: 'unauthorized-error',
  template: `<div class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
  <div class="flex flex-column align-items-center justify-content-center">
    <div
      style="border-radius:56px; padding:0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%);">
      <div class="w-full surface-card py-8 px-5 sm:px-8 flex flex-column align-items-center" style="border-radius:53px">
        <div class="grid flex flex-column align-items-center">
          <logo width="96" height="96"></logo>
          <h1 class="text-900 font-bold text-5xl mb-2">Unauthorized</h1>
          <span class="text-600 mb-5">Please login</span>
          <img src="assets/demo/images/error/asset-error.svg" alt="Error" class="mb-5" width="80%">
          <button pButton pRipple icon="pi pi-sign-in" label="Login" class="p-button-text" iconPos="right"
                  (click)="securityService.authorize()"></button>
        </div>
      </div>
    </div>
  </div>
</div>
`,
  imports: [
    ButtonModule,
    RippleModule,
    LogoComponent
  ]
})
export class UnauthorizedComponent {
  protected readonly securityService = inject(SecurityService);
}
