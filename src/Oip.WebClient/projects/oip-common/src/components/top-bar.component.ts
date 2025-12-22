import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { LayoutService } from '../services/app.layout.service';
import { AppConfiguratorComponent } from './app-configurator.component';
import { LogoComponent } from './logo.component';
import { SecurityService } from '../services/security.service';
import { TopBarService } from '../services/top-bar.service';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { AvatarModule } from 'primeng/avatar';
import { UserService } from '../services/user.service';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from "@ngx-translate/core";

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    RouterModule,
    CommonModule,
    StyleClassModule,
    AppConfiguratorComponent,
    LogoComponent,
    Tabs,
    TabList,
    Tab,
    AvatarModule,
    ButtonModule,
    TranslatePipe
  ],
  template: `
    <div class="layout-topbar">
      <div class="layout-topbar-logo-container">
        <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
          <i class="pi pi-bars"></i>
        </button>
        <a class="layout-topbar-logo" id="oip-app-topbar-logo-link" routerLink="">
          <logo [height]="36" [width]="36"></logo>
          <span>{{ 'app-info.title' | translate }}</span>
        </a>
      </div>

      @if (securityService.isAdmin() && topBarService.topBarItems.length > 0) {
        <p-tabs class="layout-topbar-tabs ml-2" [(value)]="topBarService.activeId">
          <p-tablist>
            @for (tab of topBarService.availableTopBarItems; track tab.id) {
              <p-tab id="oip-app-topbar-tab-{{ tab.id }}" [value]="tab.id">
                <i class="pi {{ tab.icon }}"></i>
                <span class="ml-2">{{ tab.caption }}</span>
              </p-tab>
            }
          </p-tablist>
        </p-tabs>
      }
      <div class="layout-topbar-actions">
        <div class="layout-config-menu">
          <p-button
            class="layout-topbar-action"
            id="oip-app-topbar-theme-button"
            severity="secondary"
            type="button"
            [rounded]="true"
            [text]="true"
            (click)="toggleDarkMode()">
            <i
              class="pi"
              [ngClass]="{
              'pi-moon': layoutService.isDarkTheme(),
              'pi-sun': !layoutService.isDarkTheme()
            }"></i>
          </p-button>
          <div class="relative">
            <p-button
              class="layout-topbar-action layout-topbar-action-highlight"
              enterActiveClass="animate-scalein"
              enterFromClass="hidden"
              id="oip-app-topbar-palette-button"
              leaveActiveClass="animate-fadeout"
              leaveToClass="hidden"
              pStyleClass="@next"
              [hideOnOutsideClick]="true"
              [rounded]="true">
              <i class="pi pi-palette"></i>
            </p-button>
            <app-configurator/>
          </div>
        </div>

        <button
          class="layout-topbar-menu-button layout-topbar-action"
          enterActiveClass="animate-scalein"
          enterFromClass="hidden"
          id="oip-app-topbar-menu-expand-button"
          leaveActiveClass="animate-fadeout"
          leaveToClass="hidden"
          pStyleClass="@next"
          [hideOnOutsideClick]="true">
          <i class="pi pi-ellipsis-v"></i>
        </button>

        <div class="layout-topbar-menu hidden lg:block">
          <div class="layout-topbar-menu-content">
            <button
              class="layout-topbar-action"
              id="oip-app-topbar-logout-button"
              type="button"
              (click)="securityService.logout()"
              (keydown)="logoutKeyDown($event)">
              <i class="pi pi-sign-out"></i>
              <span>{{ 'topbar.logout' | translate }}</span>
            </button>
            <button class="layout-topbar-action" routerLink="config">
              <p-avatar
                class="p-link flex align-items-center"
                id="oip-app-topbar-user-avatar"
                shape="circle"
                size="normal"
                [image]="userService.photoLoaded ? userService.photo : null"
              >{{ !userService.photoLoaded ? userService.shortLabel : null }}
              </p-avatar>
              <span class="ml-2">{{ 'topbar.profile' | translate }}</span>
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
    this.layoutService.layoutConfig.update((state) => ({
      ...state,
      darkTheme: !state.darkTheme
    }));
  }

  logoutKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter') {
      this.securityService.logout();
    }
  }
}
