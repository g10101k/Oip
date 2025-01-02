import { Component, OnInit, OnDestroy } from '@angular/core';
import { MenuItem, PrimeTemplate } from 'primeng/api';
import { Product } from '../../api/product';
import { ProductService } from '../../service/product.service';
import { Subscription, debounceTime } from 'rxjs';
import { BaseComponent } from "common";
import { NgIf, NgStyle, CurrencyPipe } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonDirective } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { UIChart } from 'primeng/chart';
import { AppConfigService } from "../../../layout/service/appconfigservice";

@Component({
    templateUrl: './dashboard.component.html',
    imports: [NgIf, NgStyle, TableModule, PrimeTemplate, ButtonDirective, Menu, UIChart, CurrencyPipe]
})
export class DashboardComponent extends BaseComponent implements OnInit, OnDestroy {
  items!: MenuItem[];
  products!: Product[];
  chartData: any;
  chartOptions: any;
  subscription!: Subscription;

  constructor(private productService: ProductService, public layoutService: AppConfigService) {
    super();


  }

  ngOnInit() {
    this.initChart();
    this.productService.getProductsSmall().then(data => this.products = data);

    this.items = [
      { label: 'Add New', icon: 'pi pi-fw pi-plus' },
      { label: 'Remove', icon: 'pi pi-fw pi-minus' }
    ];
    super.ngOnInit();

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

  ngOnDestroy() {
    super.ngOnDestroy();
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
