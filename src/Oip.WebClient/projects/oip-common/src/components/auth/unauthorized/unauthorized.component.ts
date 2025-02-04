import { Component, inject } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { RippleModule } from "primeng/ripple";
import { LogoComponent } from "../../logo.component";
import { SecurityService } from "../../../services/security.service";
import { AppFloatingConfigurator } from "../../app.floatingconfigurator";
import { ReactiveFormsModule } from "@angular/forms";

@Component({
  selector: 'unauthorized-error',
  template: `
    <app-floating-configurator/>
    <div
      class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
      <div class="flex flex-col items-center justify-center">

        <div
          style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
          <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">
            <div class="flex flex-col items-center justify-center">
              <logo class="" width="96" height="96"/>
            </div>
            <div class="text-center mb-8">
              <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">Welcome to OIP!</div>
              <span class="text-muted-color font-medium">Sign in to continue</span>
            </div>
            <div>
              <p-button label="Sign In" styleClass="w-full" (click)="securityService.authorize()"></p-button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [
    ButtonModule,
    RippleModule,
    LogoComponent,
    AppFloatingConfigurator,
    ReactiveFormsModule,
  ]
})
export class UnauthorizedComponent {
  protected readonly securityService = inject(SecurityService);
}
