import { Component, OnInit } from '@angular/core';
import { RippleModule } from 'primeng/ripple';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../service/product.service';
import { Product } from '../../../api/product';

@Component({
  standalone: true,
  selector: 'app-recent-sales-widget',
  imports: [CommonModule, TableModule, ButtonModule, RippleModule],
  template: `<div class="card !mb-8">
    <div class="font-semibold text-xl mb-4">Recent Sales</div>
    <p-table responsiveLayout="scroll" [paginator]="true" [rows]="5" [value]="products">
      <ng-template #header>
        <tr>
          <th>Image</th>
          <th pSortableColumn="name">Name <p-sortIcon field="name"></p-sortIcon></th>
          <th pSortableColumn="price">Price <p-sortIcon field="price"></p-sortIcon></th>
          <th>View</th>
        </tr>
      </ng-template>
      <ng-template #body let-product>
        <tr>
          <td style="width: 15%; min-width: 5rem;">
            <img
              alt="{{ product.name }}"
              class="shadow-lg"
              src="/assets/demo/images/product/{{ product.image }}"
              width="50" />
          </td>
          <td style="width: 35%; min-width: 7rem;">{{ product.name }}</td>
          <td style="width: 35%; min-width: 8rem;">
            {{ product.price | currency: 'USD' }}
          </td>
          <td style="width: 15%;">
            <button
              class="p-button p-component p-button-text p-button-icon-only"
              icon="pi pi-search"
              pButton
              pRipple
              type="button"></button>
          </td>
        </tr>
      </ng-template>
    </p-table>
  </div>`,
  providers: [ProductService]
})
export class RecentSalesWidgetComponent implements OnInit {
  products!: Product[];

  constructor(private readonly productService: ProductService) {}

  ngOnInit() {
    this.productService.getProductsSmall().then((data) => (this.products = data));
  }
}
