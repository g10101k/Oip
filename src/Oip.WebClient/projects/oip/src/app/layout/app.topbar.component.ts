import { Component, computed, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { SecurityService, TopBarService } from "common";
import { Menu } from 'primeng/menu'
import { TabView, TabPanel } from "primeng/tabview";
import { NgIf, NgFor, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AppConfigService } from "./service/appconfigservice";

@Component({
    selector: 'app-topbar',
    templateUrl: './app.topbar.component.html',
    imports: [NgIf, TabView, NgFor, TabPanel, PrimeTemplate, NgClass, Menu, RouterLink]
})
export class AppTopBarComponent implements OnInit {
  protected readonly oipSecurityService = inject(SecurityService);
  protected readonly topBarService = inject(TopBarService);
  protected readonly layoutService = inject(AppConfigService);
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

  toggleDarkMode() {
    this.layoutService.appState.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
  }

  isDarkMode = computed(() => this.layoutService.appState().darkTheme);

}
