import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Product } from '../../api/product';

@Injectable()
export class ProductService {
  http: HttpClient = inject(HttpClient);

  async getProductsSmall() {
    const res = await this.http
      .get<any>('assets/demo/data/products-small.json')
      .toPromise();
    const data = res.data as Product[];
    return data;
  }
}
