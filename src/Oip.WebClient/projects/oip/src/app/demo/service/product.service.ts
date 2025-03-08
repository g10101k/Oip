import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Product } from '../api/product';
import { lastValueFrom } from "rxjs";

@Injectable()
export class ProductService {
  http: HttpClient = inject(HttpClient);

  getProductsSmall() {
    let result = this.http.get<Product[]>('assets/demo/data/products-small.json');
    return lastValueFrom(result);
  }
}
