import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { LayoutService } from "../services/app.layout.service";
import { AppConfigurator } from "./app.configurator";
import { LogoComponent } from "./logo.component";
import { SecurityService } from "../services/security.service";
import { TopBarService } from "../services/top-bar.service";
import { Tab, TabList, Tabs } from "primeng/tabs";
import { AvatarModule } from "primeng/avatar";
import { UserService } from "../services/user.service";

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [RouterModule, CommonModule, StyleClassModule, AppConfigurator, LogoComponent, Tabs, TabList, Tab, AvatarModule],
  template: `
    <div class="layout-topbar">
      <div class="layout-topbar-logo-container">
        <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
          <i class="pi pi-bars"></i>
        </button>
        <a class="layout-topbar-logo" routerLink="">
          <logo width="36" height="36"></logo>
          <span>OIP</span>
        </a>
      </div>

      <p-tabs *ngIf="securityService.isAdmin && topBarService.topBarItems.length > 0" class="layout-topbar-tabs ml-2"
              [(value)]="topBarService.activeId">
        <p-tablist>
          @for (tab of topBarService.availableTopBarItems; track tab.id) {
            <p-tab [value]="tab.id">
              <i class="pi {{tab.icon}}"></i>
              <span>&nbsp;{{ tab.caption }}</span>
            </p-tab>
          }
        </p-tablist>
      </p-tabs>

      <div class="layout-topbar-actions">
        <div class="layout-config-menu">
          <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
            <i
              [ngClass]="{ 'pi ': true, 'pi-moon': layoutService.isDarkTheme(), 'pi-sun': !layoutService.isDarkTheme() }"></i>
          </button>
          <div class="relative">
            <button
              class="layout-topbar-action layout-topbar-action-highlight"
              pStyleClass="@next"
              enterFromClass="hidden"
              enterActiveClass="animate-scalein"
              leaveToClass="hidden"
              leaveActiveClass="animate-fadeout"
              [hideOnOutsideClick]="true"
            >
              <i class="pi pi-palette"></i>
            </button>
            <app-configurator/>
          </div>
        </div>

        <button class="layout-topbar-menu-button layout-topbar-action" pStyleClass="@next" enterFromClass="hidden"
                enterActiveClass="animate-scalein" leaveToClass="hidden" leaveActiveClass="animate-fadeout"
                [hideOnOutsideClick]="true">
          <i class="pi pi-ellipsis-v"></i>
        </button>

        <div class="layout-topbar-menu hidden lg:block">
          <div class="layout-topbar-menu-content">
            <button type="button" class="layout-topbar-action" (click)="securityService.logout()"
                    (keydown)="logoutKeyDown($event)">
              <i class="pi pi-sign-out"></i>
              <span>Logout</span>
            </button>
            <button class="layout-topbar-action" routerLink="config">
              <p-avatar class="p-link flex align-items-center"
                        [image]="userService.photoLoaded ? userService.photo : null"
                        size="normal"
                        shape="circle">{{ !userService.photoLoaded ? userService.shortLabel : null }}
              </p-avatar>
              <span class="ml-2">Profile</span>
            </button>
          </div>
        </div>
      </div>
    </div>`
})
export class AppTopbar {
  items!: MenuItem[];
  securityService = inject(SecurityService);
  topBarService = inject(TopBarService);
  userService = inject(UserService);
  layoutService = inject(LayoutService);

  toggleDarkMode() {
    this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
  }

  logoutKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter') {
      this.securityService.logout()
    }
  }
}
