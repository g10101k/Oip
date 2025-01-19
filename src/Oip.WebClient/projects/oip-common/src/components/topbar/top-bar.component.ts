import { Component, ElementRef, inject, OnInit, Sanitizer, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Menu } from 'primeng/menu'
import { SecurityService } from "../../services/security.service";
import { LayoutService } from "../../services/app.layout.service";
import { TopBarService } from "../../services/top-bar.service";
import { Router } from "@angular/router";
import { BaseDataService } from "../../services/base-data.service";
import { DomSanitizer } from "@angular/platform-browser";
import { UserService } from "../../services/user.service";

@Component({
  selector: 'top-bar',
  templateUrl: './top-bar.component.html',
})
export class TopBarComponent implements OnInit {
  readonly securityService = inject(SecurityService);
  readonly layoutService = inject(LayoutService);
  readonly topBarService = inject(TopBarService);
  readonly userService = inject(UserService);
  readonly router: Router = inject(Router);
  items: MenuItem[] = [];

  @ViewChild('menuButton') menuButton!: ElementRef;
  @ViewChild('topbarmenubutton') topbarMenuButton!: ElementRef;
  @ViewChild('topbarmenu') menu!: ElementRef;
  @ViewChild('userMenu') userMenu!: Menu;

  ngOnInit() {
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
              command: () => this.securityService.logout()
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
              command: () => this.securityService.authorize()
            }
          ]
        }
      ]
    }
  }

  userMenuClick($event: any) {
    this.processToken(this.securityService.token.getValue());
    this.userMenu.toggle($event);
  }
}
