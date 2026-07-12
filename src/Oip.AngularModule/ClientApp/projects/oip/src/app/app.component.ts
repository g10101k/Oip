import { Component, inject, OnInit } from '@angular/core';
import { SecurityService, L10nService, NotificationService } from 'oip-common';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';

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
  private readonly notificationService = inject(NotificationService);

  ngOnInit() {
    this.securityService.auth();
    this.translateService.init([
      {
        code: 'en',
        name: 'English',
        icon: 'flag flag-gb'
      },
      {
        code: 'ru',
        name: 'Русский',
        icon: 'flag flag-ru'
      }
    ]);
  }
}
