import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Menu } from 'primeng/menu'
import { SecurityService } from "../../services/security.service";
import { LayoutService } from "../../services/app.layout.service";
import { TopBarService } from "../../services/top-bar.service";
import { Router } from "@angular/router";

@Component({
  selector: 'top-bar',
  templateUrl: './top-bar.component.html',
})
export class TopBarComponent implements OnInit {
  protected readonly oipSecurityService = inject(SecurityService);
  protected readonly layoutService = inject(LayoutService);
  protected readonly topBarService = inject(TopBarService);
  readonly router: Router = inject(Router);
  items: MenuItem[] = [];

  @ViewChild('menuButton') menuButton!: ElementRef;
  @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;
  @ViewChild('topbarmenu') menu!: ElementRef;
  @ViewChild('userMenu') userMenu!: Menu;

  ngOnInit() {
    // on init
  }

  processToken(token: any) {
    if (token?.email) {
      this.items = [
        {
          label: `${token.name}`,
          items: [{
            label: 'Config',
            icon: 'pi pi-cog',
            command: () => this.router.navigate(['/config'])
          },
            {
              label: 'Logout',
              icon: 'pi pi-sign-out',
              command: () => this.oipSecurityService.logout()
            },
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

  userMenuClick($event: any) {
    this.processToken(this.oipSecurityService.token.getValue());
    this.userMenu.toggle($event);
  }
}
