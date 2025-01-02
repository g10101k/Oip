import { Component, OnInit } from '@angular/core';
import { SelectItem, PrimeTemplate } from 'primeng/api';
import { DataView } from 'primeng/dataview';
import { Product } from 'projects/oip/src/app/demo/api/product';
import { ProductService } from 'projects/oip/src/app/demo/service/product.service';
import { DropdownModule } from 'primeng/dropdown';
import { InputText } from 'primeng/inputtext';
import { NgFor } from '@angular/common';
import { Rating } from 'primeng/rating';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { PickList } from 'primeng/picklist';
import { OrderList } from 'primeng/orderlist';

@Component({
    templateUrl: './listdemo.component.html',
    imports: [DataView, PrimeTemplate, DropdownModule, InputText, NgFor, Rating, FormsModule, Button, PickList, OrderList]
})
export class ListDemoComponent implements OnInit {

  products: Product[] = [];

  sortOptions: SelectItem[] = [];

  sortOrder: number = 0;

  sortField: string = '';

  sourceCities: any[] = [];

  targetCities: any[] = [];

  orderCities: any[] = [];

  constructor(private productService: ProductService) {
  }

  ngOnInit() {
    this.productService.getProducts().then(data => this.products = data);

    this.sourceCities = [
      { name: 'San Francisco', code: 'SF' },
      { name: 'London', code: 'LDN' },
      { name: 'Paris', code: 'PRS' },
      { name: 'Istanbul', code: 'IST' },
      { name: 'Berlin', code: 'BRL' },
      { name: 'Barcelona', code: 'BRC' },
      { name: 'Rome', code: 'RM' }];

    this.targetCities = [];

    this.orderCities = [
      { name: 'San Francisco', code: 'SF' },
      { name: 'London', code: 'LDN' },
      { name: 'Paris', code: 'PRS' },
      { name: 'Istanbul', code: 'IST' },
      { name: 'Berlin', code: 'BRL' },
      { name: 'Barcelona', code: 'BRC' },
      { name: 'Rome', code: 'RM' }];

    this.sortOptions = [
      { label: 'Price High to Low', value: '!price' },
      { label: 'Price Low to High', value: 'price' }
    ];
  }

  onSortChange(event: any) {
    const value = event.value;

    if (value.indexOf('!') === 0) {
      this.sortOrder = -1;
      this.sortField = value.substring(1, value.length);
    } else {
      this.sortOrder = 1;
      this.sortField = value;
    }
  }

  onFilter(dv: DataView, event: Event) {
    dv.filter((event.target as HTMLInputElement).value);
  }

}
