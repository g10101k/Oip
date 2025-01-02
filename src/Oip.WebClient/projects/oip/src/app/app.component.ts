import { Component, inject, OnInit } from '@angular/core';
import { AuthConfigModule, SecurityService } from "common";
import { CommonModule, LocationStrategy, PathLocationStrategy } from "@angular/common";
import { CountryService } from "./demo/service/country.service";
import { CustomerService } from "./demo/service/customer.service";
import { EventService } from "./demo/service/event.service";
import { IconService } from "./demo/service/icon.service";
import { NodeService } from "./demo/service/node.service";
import { PhotoService } from "./demo/service/photo.service";
import { ProductService } from "./demo/service/product.service";
import { AuthGuardService } from "./services/auth.service";
import { MessageService } from "primeng/api";
import { AppLayoutModule } from "./layout/app.layout.module";
import { ToastModule } from "primeng/toast";
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [ AppLayoutModule, AuthConfigModule, ToastModule , RouterOutlet],
  providers: [
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    CountryService, CustomerService, EventService, IconService, NodeService,
    PhotoService, ProductService, AuthGuardService, MessageService,
  ],
})
export class AppComponent implements OnInit {
  private readonly oipSecurityService = inject(SecurityService);

  constructor() {
  }

  ngOnInit() {
    this.oipSecurityService.auth();
  }
}
