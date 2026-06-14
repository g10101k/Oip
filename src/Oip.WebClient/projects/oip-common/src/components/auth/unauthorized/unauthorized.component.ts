import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { LogoComponent } from '../../logo.component';
import { BffSecurityService, SecurityService } from '../../../services/security.service';
import { AppFloatingConfiguratorComponent } from '../../app-floating-configurator.component';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  template: `
    <app-floating-configurator />
    <div
      class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
      <div class="flex flex-col items-center justify-center">
        <div
          style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)">
          <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">
            <div class="flex flex-col items-center justify-center">
              <logo [height]="96" [width]="96" />
            </div>
            <div class="text-center mb-8">
              <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
                {{ 'unauthorized.welcomeToOip' | translate }}
              </div>
              <span class="text-muted-color font-medium">{{ 'unauthorized.signInToContinue' | translate }}</span>
            </div>
            <div>
              <p-button
                id="oip-unauthorized-error-sign-in-button"
                label="{{ 'unauthorized.signIn' | translate }}"
                styleClass="w-full"
                (click)="signIn()"></p-button>
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
    AppFloatingConfiguratorComponent,
    ReactiveFormsModule,
    TranslatePipe
  ]
})
export class UnauthorizedComponent implements OnInit {
  protected readonly securityService = inject(SecurityService);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void {
    const returnUrl = sessionStorage.getItem(BffSecurityService.authorizeAfterLogoutReturnUrlKey);
    if (!returnUrl) {
      return;
    }

    sessionStorage.removeItem(BffSecurityService.authorizeAfterLogoutReturnUrlKey);
    this.securityService.authorize(returnUrl);
  }

  protected signIn(): void {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
    this.securityService.authorize(returnUrl);
  }
}
