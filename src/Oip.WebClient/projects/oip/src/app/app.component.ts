import { Component, inject, OnInit } from '@angular/core';
import { SecurityService } from 'oip-common';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { L10nService } from '../../../oip-common/src/services/l10n.service';

@Component({
  selector: 'app-root',
  template: `
    <p-toast />
    <router-outlet></router-outlet>
  `,
  standalone: true,
  imports: [ToastModule, RouterOutlet]
})
export class AppComponent implements OnInit {
  private readonly securityService = inject(SecurityService);
  private readonly translateService = inject(L10nService);

  ngOnInit() {
    this.securityService.auth();
    this.translateService.init(['en', 'ru']);
  }
}
