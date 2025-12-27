import { Component } from '@angular/core';
import { LogoComponent } from './logo.component';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-footer',
  template: `
    <div class="layout-footer">
      <div class="flex justify-center flex-1">
        <logo class="mr-2 -my-0.5" [height]="18" [width]="18"></logo>
        <span class="font-medium">{{ 'app-info.footer' | translate }}</span>
      </div>
      <p class="mr-auto">{{ 'app-info.version' | translate }}</p>
    </div>
  `,
  standalone: true,
  imports: [LogoComponent, TranslatePipe]
})
export class FooterComponent {}
