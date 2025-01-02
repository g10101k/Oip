import { Component, OnInit } from '@angular/core';
import { ConfirmationService, MessageService, PrimeTemplate } from 'primeng/api';
import { Product } from 'projects/oip/src/app/demo/api/product';
import { ProductService } from 'projects/oip/src/app/demo/service/product.service';
import { Toast } from 'primeng/toast';
import { Dialog } from 'primeng/dialog';
import { ButtonDirective } from 'primeng/button';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { TableModule } from 'primeng/table';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Sidebar } from 'primeng/sidebar';
import { InputText } from 'primeng/inputtext';
import { Tooltip } from 'primeng/tooltip';
import { Ripple } from 'primeng/ripple';
import { ConfirmPopup } from 'primeng/confirmpopup';

@Component({
    templateUrl: './overlaysdemo.component.html',
    providers: [ConfirmationService, MessageService],
    imports: [
        Toast,
        Dialog,
        PrimeTemplate,
        ButtonDirective,
        OverlayPanelModule,
        TableModule,
        ConfirmDialog,
        Sidebar,
        InputText,
        Tooltip,
        Ripple,
        ConfirmPopup,
    ],
})
export class OverlaysDemoComponent implements OnInit {

  images: any[] = [];

  display: boolean = false;

  products: Product[] = [];

  selectedProduct: Product = {};

  visibleSidebar1: boolean = false;

  visibleSidebar2: boolean = false;

  visibleSidebar3: boolean = false;

  visibleSidebar4: boolean = false;

  visibleSidebar5: boolean = false;

  constructor(private productService: ProductService, private confirmationService: ConfirmationService, private messageService: MessageService) {
  }

  ngOnInit() {
    this.productService.getProductsSmall().then(products => this.products = products);

    this.images = [];
    this.images.push({
      source: 'assets/demo/images/sopranos/sopranos1.jpg',
      thumbnail: 'assets/demo/images/sopranos/sopranos1_small.jpg', title: 'Sopranos 1'
    });
    this.images.push({
      source: 'assets/demo/images/sopranos/sopranos2.jpg',
      thumbnail: 'assets/demo/images/sopranos/sopranos2_small.jpg', title: 'Sopranos 2'
    });
    this.images.push({
      source: 'assets/demo/images/sopranos/sopranos3.jpg',
      thumbnail: 'assets/demo/images/sopranos/sopranos3_small.jpg', title: 'Sopranos 3'
    });
    this.images.push({
      source: 'assets/demo/images/sopranos/sopranos4.jpg',
      thumbnail: 'assets/demo/images/sopranos/sopranos4_small.jpg', title: 'Sopranos 4'
    });
  }

  confirm1() {
    this.confirmationService.confirm({
      key: 'confirm1',
      message: 'Are you sure to perform this action?'
    });
  }

  confirm2(event: Event) {
    this.confirmationService.confirm({
      key: 'confirm2',
      target: event.target || new EventTarget,
      message: 'Are you sure that you want to proceed?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.messageService.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted' });
      },
      reject: () => {
        this.messageService.add({ severity: 'error', summary: 'Rejected', detail: 'You have rejected' });
      }
    });
  }

  formatCurrency(value: number) {
    return value.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
  }

}
