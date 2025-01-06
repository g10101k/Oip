import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { LayoutService } from "./service/app.layout.service";
import { SecurityService, TopBarService } from "oip/common";
import { Menu } from 'primeng/menu'
import { OidcSecurityService } from "angular-auth-oidc-client";
import { TabView } from "primeng/tabview";

@Component({
  selector: 'app-topbar',
  templateUrl: './app.topbar.component.html'
})
export class AppTopBarComponent implements OnInit {
  protected readonly oipSecurityService = inject(SecurityService);
  protected readonly layoutService = inject(LayoutService);
  protected readonly topBarService = inject(TopBarService);

  items: MenuItem[] = [];

  @ViewChild('menubutton') menuButton!: ElementRef;
  @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;
  @ViewChild('topbarmenu') menu!: ElementRef;
  @ViewChild('userMenu') userMenu!: Menu;

  ngOnInit() {

  }

  processToken(token: any) {
    if (token && token.email) {
      this.items = [
        {
          label: `${token.name}`,
          items: [
            {
              label: 'Logout',
              icon: 'pi pi-sign-out',
              command: () => this.oipSecurityService.logout()
            }
          ]
        }
      ]
    } else {
      this.items = [
        {
          label: `Need login`,
          items: [
            {
              label: 'Login',
              icon: 'pi pi-sign-in',
              command: () => this.oipSecurityService.authorize()
            }
          ]
        }
      ]
    }
  }

  click() {
    this.oipSecurityService.logoff().subscribe((result) => console.log(result));
  }

  userMenuClick($event:any){
    this.processToken(this.oipSecurityService.token.getValue());
    this.userMenu.toggle($event);
  }
}
