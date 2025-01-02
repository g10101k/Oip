import { Component, OnInit } from '@angular/core';
import { ProductService } from 'projects/oip/src/app/demo/service/product.service';
import { PhotoService } from 'projects/oip/src/app/demo/service/photo.service';
import { Product } from 'projects/oip/src/app/demo/api/product';
import { Carousel } from 'primeng/carousel';
import { PrimeTemplate } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Image } from 'primeng/image';
import { GalleriaModule } from 'primeng/galleria';

@Component({
    templateUrl: './mediademo.component.html',
    imports: [Carousel, PrimeTemplate, ButtonDirective, Image, GalleriaModule]
})
export class MediaDemoComponent implements OnInit {

  products!: Product[];

  images!: any[];

  galleriaResponsiveOptions: any[] = [
    {
      breakpoint: '1024px',
      numVisible: 5
    },
    {
      breakpoint: '960px',
      numVisible: 4
    },
    {
      breakpoint: '768px',
      numVisible: 3
    },
    {
      breakpoint: '560px',
      numVisible: 1
    }
  ];

  carouselResponsiveOptions: any[] = [
    {
      breakpoint: '1024px',
      numVisible: 3,
      numScroll: 3
    },
    {
      breakpoint: '768px',
      numVisible: 2,
      numScroll: 2
    },
    {
      breakpoint: '560px',
      numVisible: 1,
      numScroll: 1
    }
  ];

  constructor(private productService: ProductService, private photoService: PhotoService) {
  }

  ngOnInit() {
    this.productService.getProductsSmall().then(products => {
      this.products = products;
    });

    this.photoService.getImages().then(images => {
      this.images = images;
    });
  }

}
