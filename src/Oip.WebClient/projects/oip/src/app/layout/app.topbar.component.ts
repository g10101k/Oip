import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from "./service/app.layout.service";
import { TopBarService } from "common";
import { OidcSecurityService } from "angular-auth-oidc-client";
import { TabView } from "primeng/tabview";

@Component({
  selector: 'app-topbar',
  templateUrl: './app.topbar.component.html'
})
export class AppTopBarComponent {

  protected readonly oidcSecurityService = inject(OidcSecurityService);
  protected readonly layoutService = inject(LayoutService);
  protected readonly topBarService = inject(TopBarService);

  items!: MenuItem[];

  @ViewChild('menubutton') menuButton!: ElementRef;

  @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;

  @ViewChild('topbarmenu') menu!: ElementRef;

}
