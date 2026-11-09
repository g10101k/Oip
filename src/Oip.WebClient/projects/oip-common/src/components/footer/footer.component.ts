import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { LogoComponent } from '../logo/logo.component';
import { AppInfoService } from '../../services/app-info.service';

@Component({
  selector: 'app-footer',
  template: `
    <div class="layout-footer flex-wrap">
      <div class="flex min-w-0 flex-1 items-center justify-center gap-2">
        <logo class="logo-footer" />
        <span class="min-w-0 break-words font-medium">{{ appInfoService.getFooter() ?? ('app-info.footer' | translate) }}</span>
      </div>
      <p class="mr-auto">{{ appInfoService.getVersion() ?? ('app-info.version' | translate) }}</p>
    </div>
  `,
  standalone: true,
  imports: [TranslatePipe, LogoComponent]
})
export class FooterComponent {
  protected appInfoService = inject(AppInfoService);
}
