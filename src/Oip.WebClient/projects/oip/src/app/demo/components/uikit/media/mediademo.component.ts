import { Component, OnInit } from '@angular/core';
import { ProductService } from 'projects/oip/src/app/demo/service/product.service';
import { PhotoService } from 'projects/oip/src/app/demo/service/photo.service';
import { Product } from 'projects/oip/src/app/demo/api/product';
import { GalleriaModule } from 'primeng/galleria';
import { ImageModule } from 'primeng/image';
import { ButtonModule } from 'primeng/button';
import { SharedModule } from 'primeng/api';
import { CarouselModule } from 'primeng/carousel';

@Component({
    templateUrl: './mediademo.component.html',
    standalone: true,
    imports: [CarouselModule, SharedModule, ButtonModule, ImageModule, GalleriaModule]
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

    constructor(private readonly productService: ProductService, private readonly photoService: PhotoService) { }

    ngOnInit() {
        this.productService.getProductsSmall().then(products => {
            this.products = products;
        });

        this.photoService.getImages().then(images => {
            this.images = images;
        });
    }

}
