import { Component } from '@angular/core';
import { NotificationsWidget } from './components/notificationswidget';
import { StatsWidget } from './components/statswidget';
import { RecentSalesWidget } from './components/recentsaleswidget';
import { BestSellingWidget } from './components/bestsellingwidget';
import { RevenueStreamWidget } from './components/revenuestreamwidget';
import { BaseComponent, Feature, SecurityComponent } from "oip-common";
import { NgIf } from "@angular/common";

interface DashboardSettingsDto {
}

@Component({
  selector: 'app-dashboard',
  imports: [StatsWidget, RecentSalesWidget, BestSellingWidget, RevenueStreamWidget, NotificationsWidget, NgIf, SecurityComponent],
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
export class Dashboard extends BaseComponent<DashboardSettingsDto> implements Feature {
  controller: string = 'dashboard';
}

