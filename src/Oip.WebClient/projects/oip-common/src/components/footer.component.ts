import { Component, inject } from '@angular/core';
import { LogoComponent } from './logo.component';
import { TranslatePipe } from '@ngx-translate/core';
import { NgComponentOutlet } from '@angular/common';
import { LogoService } from '../services/logo.service';

@Component({
  selector: 'app-footer',
  template: `
    <div class="layout-footer">
      <div class="flex justify-center flex-1">
        <div class="mr-2 -my-0.5">
          <ng-container
            *ngComponentOutlet="logoService.getLogoComponent(); inputs: { width: 18, height: 18 }"></ng-container>
        </div>
        <span class="font-medium">{{ 'app-info.footer' | translate }}</span>
      </div>
      <p class="mr-auto">{{ 'app-info.version' | translate }}</p>
    </div>
  `,
  standalone: true,
  imports: [TranslatePipe, NgComponentOutlet]
})
export class FooterComponent {
  protected logoService = inject(LogoService);
}
