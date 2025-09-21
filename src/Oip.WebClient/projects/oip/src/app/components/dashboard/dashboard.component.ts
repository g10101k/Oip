import { Component } from '@angular/core';
import { NotificationsWidgetComponent } from './components/notifications-widget.component';
import { StatsWidget } from './components/statswidget';
import { RecentSalesWidget } from './components/recentsaleswidget';
import { BestSellingWidget } from './components/bestsellingwidget';
import { RevenueStreamWidget } from './components/revenuestreamwidget';
import { BaseModuleComponent, NoSettingsDto, SecurityComponent } from "oip-common";
import { NgIf } from "@angular/common";

@Component({
  selector: 'app-dashboard',
  imports: [StatsWidget, RecentSalesWidget, BestSellingWidget, RevenueStreamWidget, NotificationsWidgetComponent, NgIf, SecurityComponent],
  template: `
    <div *ngIf="isContent" class="grid grid-cols-12 gap-4">
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
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
  `
})
export class DashboardComponent extends BaseModuleComponent<NoSettingsDto, NoSettingsDto> {
}

