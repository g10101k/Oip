import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Product } from '../api/product';

@Injectable()
export class ProductService {
  http: HttpClient = inject(HttpClient);

  getProductsSmall() {
    return this.http
      .get<any>('assets/demo/data/products-small.json')
      .toPromise()
      .then((res) => res.data as Product[])
      .then((data) => data);
  }
}
