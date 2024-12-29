import { Component, OnDestroy, Renderer2, ViewChild, inject, OnInit } from '@angular/core';
import { PrimeNGConfig } from 'primeng/api';
import { LoginResponse, OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  private readonly primengConfig = inject(PrimeNGConfig);
  private readonly oidcSecurityService = inject(OidcSecurityService);


  constructor() {
  }

  ngOnInit() {
    this.oidcSecurityService.checkAuth().subscribe((loginResponse: LoginResponse) => {
    });

    this.primengConfig.ripple = true;
  }
}
