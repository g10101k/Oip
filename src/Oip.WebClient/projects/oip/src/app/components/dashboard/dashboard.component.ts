import { Component } from '@angular/core';
import { NotificationsWidgetComponent } from './components/notifications-widget.component';
import { StatsWidgetComponent } from './components/stats-widget.component';
import { RecentSalesWidgetComponent } from './components/recent-sales-widget.component';
import { BestSellingWidgetComponent } from './components/best-selling-widget.component';
import { RevenueStreamWidgetComponent } from './components/revenue-stream-widget.component';
import { BaseModuleComponent, NoSettingsDto, SecurityComponent } from 'oip-common';

@Component({
  selector: 'app-dashboard',
  imports: [
    StatsWidgetComponent,
    RecentSalesWidgetComponent,
    BestSellingWidgetComponent,
    RevenueStreamWidgetComponent,
    NotificationsWidgetComponent,
    SecurityComponent
  ],
  template: `
    @if (isContent) {
      <div class="grid grid-cols-12 gap-4">
        <app-stats-widget class="contents"/>
        <div class="col-span-12 xl:col-span-6">
          <app-recent-sales-widget/>
          <app-best-selling-widget/>
        </div>
        <div class="col-span-12 xl:col-span-6">
          <app-revenue-stream-widget/>
          <app-notifications-widget/>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"></security>
    }
  `
})
export class DashboardComponent extends BaseModuleComponent<NoSettingsDto, NoSettingsDto> {}
