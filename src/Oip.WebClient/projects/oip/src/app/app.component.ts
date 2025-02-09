import { Component, inject, OnInit } from '@angular/core';
import { LayoutService, SecurityService } from "oip-common";
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { TranslateService } from '@ngx-translate/core';
import { PrimeNG } from 'primeng/config';

@Component({
  selector: 'app-root',
  template: `
    <p-toast/>
    <router-outlet></router-outlet>
  `,
  standalone: true,
  imports: [ToastModule, RouterOutlet]
})
export class AppComponent implements OnInit {
  private readonly securityService = inject(SecurityService);
  private readonly translateService = inject(TranslateService)
  private readonly primeNgConfig = inject(PrimeNG);
  private readonly layoutService = inject(LayoutService);

  constructor() {
  }

  ngOnInit() {
    this.translateService.addLangs(['en', 'ru']);
    let lang = /en|ru/.exec(this.layoutService.language()) ? this.layoutService.language() : 'en';
    this.translateService.setDefaultLang(lang);
    this.translate(lang);
    this.securityService.auth();
  }

  translate(lang: string) {
    this.translateService.use(lang);
    this.translateService.get('primeng').subscribe(res => this.primeNgConfig.setTranslation(res));
  }
}
