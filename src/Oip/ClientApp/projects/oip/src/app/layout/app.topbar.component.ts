import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from "./service/app.layout.service";
import { ModuleTopBarService } from "shared-lib";
import { OidcSecurityService } from "angular-auth-oidc-client";

@Component({
  selector: 'app-topbar',
  templateUrl: './app.topbar.component.html'
})
export class AppTopBarComponent {
  private readonly oidcSecurityService = inject(OidcSecurityService);

  items!: MenuItem[];

  @ViewChild('menubutton') menuButton!: ElementRef;

  @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;

  @ViewChild('topbarmenu') menu!: ElementRef;

  constructor(public layoutService: LayoutService, public topBarService: ModuleTopBarService) {
  }

  login() {
    this.oidcSecurityService.authorize();
  }
}
