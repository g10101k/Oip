import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { ApplicationsApi } from '../api/applications.api';
import { ApplicationRegistryItemDto } from '../api/applications-data-contracts';
import { ApplicationRegistryService } from '../services/application-registry.service';

@Component({
  selector: 'app-topbar-application-switcher',
  standalone: true,
  imports: [CommonModule, ButtonModule, MenuModule, TranslatePipe],
  template: `
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
      <p-menu #applicationMenu [model]="applicationMenuItems" [popup]="true" />
    }
  `,
  providers: [ApplicationsApi, ApplicationRegistryService]
})
export class AppTopbarApplicationSwitcherComponent implements OnInit {
  protected applicationMenuItems: MenuItem[] = [];
  protected readonly applicationRegistryService = inject(ApplicationRegistryService);

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
