import { Component, inject, OnInit } from '@angular/core';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { LayoutService } from '../services/app.layout.service';
import { AppConfiguratorComponent } from './app-configurator.component';
import { SecurityService } from '../services/security.service';
import { TopBarService } from '../services/top-bar.service';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { AvatarModule } from 'primeng/avatar';
import { UserService } from '../services/user.service';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { LogoService } from '../services/logo.service';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { UserNotificationsComponent } from './user-notifications.component';
import { MenuModule } from 'primeng/menu';
import { ApplicationsApi } from '../api/applications.api';
import { ApplicationRegistryItemDto } from '../api/data-contracts'
import { ApplicationRegistryService } from '../services/application-registry.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    RouterModule,
    CommonModule,
    StyleClassModule,
    AppConfiguratorComponent,
    Tabs,
    TabList,
    Tab,
    AvatarModule,
    ButtonModule,
    ConfirmDialog,
    UserNotificationsComponent,
    TranslatePipe,
    MenuModule
  ],
  template: `
    <div class="layout-topbar">
      <p-confirmDialog appendTo="body"/>
      <div class="layout-topbar-logo-container">
        <button class="layout-menu-button layout-topbar-action" (click)="layoutService.onMenuToggle()">
          <i class="pi pi-bars"></i>
        </button>
        <a class="layout-topbar-logo" id="oip-app-topbar-logo-link" routerLink="">
          <ng-container
            *ngComponentOutlet="logoService.getLogoComponent(); inputs: { width: 36, height: 36 }"></ng-container>
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
          <app-user-notifications/>
          @if (applicationRegistryService.applications.length > 1) {
            <p-button
              class="layout-topbar-action"
              id="oip-app-topbar-application-switcher-button"
              severity="secondary"
              type="button"
              [rounded]="true"
              [text]="true"
              (click)="applicationMenu.toggle($event)">
              <i [ngClass]="applicationRegistryService.currentApplication?.icon || 'pi pi-th-large'"></i>
              <span class="ml-2 hidden md:inline">
              {{ applicationRegistryService.currentApplication?.displayName || ('topbar.applications' | translate) }}
            </span>
            </p-button>
            <p-menu #applicationMenu [model]="applicationMenuItems" [popup]="true"/>
          }
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
              (click)="confirmLogout()"
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
    </div>`,
  providers: [ConfirmationService, ApplicationsApi, ApplicationRegistryService]
})
export class AppTopbar implements OnInit {
  items!: MenuItem[];
  applicationMenuItems: MenuItem[] = [];
  securityService = inject(SecurityService);
  topBarService = inject(TopBarService);
  userService = inject(UserService);
  layoutService = inject(LayoutService);
  logoService = inject(LogoService);
  applicationRegistryService = inject(ApplicationRegistryService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translateService = inject(TranslateService);

  async ngOnInit(): Promise<void> {
    try {
      await this.applicationRegistryService.loadApplications();
      this.applicationMenuItems = this.applicationRegistryService.applications.map((application) =>
        this.createApplicationMenuItem(application)
      );
    } catch {
      this.applicationMenuItems = [];
    }
  }

  toggleDarkMode() {
    this.layoutService.layoutConfig.update((state) => ({
      ...state,
      darkTheme: !state.darkTheme
    }));
  }

  logoutKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter') {
      $event.preventDefault();
      this.confirmLogout();
    }
  }

  confirmLogout() {
    this.confirmationService.confirm({
      header: this.translateService.instant('topbar.logoutConfirmHeader'),
      message: this.translateService.instant('topbar.logoutConfirmMessage'),
      icon: 'pi pi-sign-out',
      rejectButtonProps: {
        label: this.translateService.instant('topbar.logoutConfirmCancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.translateService.instant('topbar.logoutConfirmAccept'),
        severity: 'danger'
      },
      accept: () => {
        this.securityService.logout();
      }
    });
  }

  private createApplicationMenuItem(application: ApplicationRegistryItemDto): MenuItem {
    return {
      id: `oip-app-switcher-${application.code}`,
      label: application.displayName ?? '',
      icon: application.icon ?? undefined,
      disabled: application.isCurrent,
      command: () => this.applicationRegistryService.openApplication(application)
    };
  }
}
