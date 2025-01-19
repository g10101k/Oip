import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { MenuItem, SharedModule } from 'primeng/api';
import { Menu, MenuModule } from 'primeng/menu'
import { SecurityService } from "../../services/security.service";
import { LayoutService } from "../../services/app.layout.service";
import { TopBarService } from "../../services/top-bar.service";
import { Router, RouterLink } from "@angular/router";
import { UserService } from "../../services/user.service";
import { AvatarModule } from 'primeng/avatar';
import { TabViewModule } from 'primeng/tabview';
import { NgIf, NgFor, NgClass } from '@angular/common';
import { LogoComponent } from '../logo/logo.component';

@Component({
  selector: 'top-bar',
  templateUrl: './top-bar.component.html',
  standalone: true,
  imports: [
    RouterLink,
    LogoComponent,
    NgIf,
    TabViewModule,
    NgFor,
    SharedModule,
    MenuModule,
    NgClass,
    AvatarModule,

  ],
})
export class TopBarComponent {
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

  processToken(token: any) {
    if (token?.email) {
      this.items = [
        {
          label: `${token.name}`,
          items: [{
            label: 'Config',
            icon: 'pi pi-cog',
            command: () => this.toUserConfig()
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

  toUserConfig() {
    this.router.navigate(['/config']).then().catch();
  }

  userMenuClick($event: any) {
    this.processToken(this.securityService.token.getValue());
    this.userMenu.toggle($event);
  }
}
