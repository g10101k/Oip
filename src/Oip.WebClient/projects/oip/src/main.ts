import { enableProdMode, importProvidersFrom } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';


import { environment } from './environments/environment';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { ToastModule } from 'primeng/toast';
import { AppRoutingModule } from './app/app-routing.module';
import { MessageService } from 'primeng/api';
import { AuthGuardService, SecurityDataService, BaseDataService, UserService, AuthConfigModule } from 'oip-common';
import { ProductService } from './app/demo/service/product.service';
import { PhotoService } from './app/demo/service/photo.service';
import { NodeService } from './app/demo/service/node.service';
import { IconService } from './app/demo/service/icon.service';
import { EventService } from './app/demo/service/event.service';
import { CustomerService } from './app/demo/service/customer.service';
import { CountryService } from './app/demo/service/country.service';
import { LocationStrategy, PathLocationStrategy } from '@angular/common';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
    providers: [
        importProvidersFrom(AppRoutingModule, AuthConfigModule, ToastModule),
        { provide: LocationStrategy, useClass: PathLocationStrategy },
        CountryService, CustomerService, EventService, IconService, NodeService,
        PhotoService, ProductService, AuthGuardService, MessageService, SecurityDataService, BaseDataService, UserService,
    ]
})
  .catch(err => console.error(err));
