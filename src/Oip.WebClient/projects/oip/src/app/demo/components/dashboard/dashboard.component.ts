import { Component, OnInit, OnDestroy } from '@angular/core';
import { MenuItem, SharedModule } from 'primeng/api';
import { Product } from '../../api/product';
import { ProductService } from '../../service/product.service';
import { Subscription, debounceTime } from 'rxjs';
import { LayoutService, BaseComponent, Feature } from "oip-common";
import { DashboardSettingsDto } from "./dashboard-settings.dto";
import { SecurityComponent } from '../../../../../../oip-common/src/components/security/security.component';
import { ChartModule } from 'primeng/chart';
import { MenuModule } from 'primeng/menu';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { NgIf, NgStyle, CurrencyPipe } from '@angular/common';

@Component({
    templateUrl: './dashboard.component.html',
    standalone: true,
    imports: [
        NgIf,
        NgStyle,
        TableModule,
        SharedModule,
        ButtonModule,
        MenuModule,
        ChartModule,
        SecurityComponent,
        CurrencyPipe,
    ],
})
export class DashboardComponent extends BaseComponent<DashboardSettingsDto> implements OnInit, OnDestroy, Feature {
  items!: MenuItem[];
  products!: Product[];
  chartData: any;
  chartOptions: any;
  subscription!: Subscription;

  constructor(private readonly productService: ProductService, public layoutService: LayoutService) {
    super();
    this.subscription = this.layoutService.configUpdate$
      .pipe(debounceTime(25))
      .subscribe((config) => {
        this.initChart();
      });
  }

  async ngOnInit() {
    await super.ngOnInit();

    this.initChart();
    this.productService.getProductsSmall().then(data => this.products = data);

    this.items = [
      { label: 'Add New', icon: 'pi pi-fw pi-plus' },
      { label: 'Remove', icon: 'pi pi-fw pi-minus' }
    ];
  }

  initChart() {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

    this.chartData = {
      labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
      datasets: [
        {
          label: 'First Dataset',
          data: [65, 59, 80, 81, 56, 55, this.id],
          fill: false,
          backgroundColor: documentStyle.getPropertyValue('--bluegray-700'),
          borderColor: documentStyle.getPropertyValue('--bluegray-700'),
          tension: .4
        },
        {
          label: 'Second Dataset',
          data: [28, 48, 40, 19, 86, 27, 90],
          fill: false,
          backgroundColor: documentStyle.getPropertyValue('--green-600'),
          borderColor: documentStyle.getPropertyValue('--green-600'),
          tension: .4
        }
      ]
    };

    this.chartOptions = {
      plugins: {
        legend: {
          labels: {
            color: textColor
          }
        }
      },
      scales: {
        x: {
          ticks: {
            color: textColorSecondary
          },
          grid: {
            color: surfaceBorder,
            drawBorder: false
          }
        },
        y: {
          ticks: {
            color: textColorSecondary
          },
          grid: {
            color: surfaceBorder,
            drawBorder: false
          }
        }
      }
    };
  }

  async ngOnDestroy() {
    await super.ngOnDestroy();
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  controller: string = 'dashboard';
}
