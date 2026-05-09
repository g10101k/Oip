import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { L10nService, SecurityService } from 'oip-common';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastModule],
  template: `
    <p-toast />
    <router-outlet></router-outlet>
  `
})
export class App implements OnInit {
  private readonly securityService = inject(SecurityService);
  private readonly l10nService = inject(L10nService);

  ngOnInit(): void {
    this.securityService.auth();
    this.l10nService.init([
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
