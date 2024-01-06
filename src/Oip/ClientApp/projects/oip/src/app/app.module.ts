import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { InputTextModule } from 'primeng/inputtext';
import { SidebarModule } from 'primeng/sidebar';
import { BadgeModule } from 'primeng/badge';
import { RadioButtonModule } from 'primeng/radiobutton';
import { InputSwitchModule } from 'primeng/inputswitch';
import { RippleModule } from 'primeng/ripple';
import { AppMenuComponent } from './layout/app.menu.component';
import { AppMenuitemComponent } from './layout/app.menuitem.component';
import { RouterModule } from '@angular/router';
import { AppTopBarComponent } from './layout/app.topbar.component';
import { AppFooterComponent } from './layout/app.footer.component';
import { AppConfigModule } from './layout/config/config.module';
import { AppSidebarComponent } from "./layout/app.sidebar.component";
import { AppLayoutComponent } from "./layout/app.layout.component";
import { AppComponent } from "./app.component";
import { NotfoundComponent } from "./demo/components/notfound/notfound.component";
import { HashLocationStrategy, LocationStrategy, CommonModule } from "@angular/common";
import { CountryService } from "./demo/service/country.service";
import { CustomerService } from "./demo/service/customer.service";
import { EventService } from "./demo/service/event.service";
import { IconService } from "./demo/service/icon.service";
import { NodeService } from "./demo/service/node.service";
import { PhotoService } from "./demo/service/photo.service";
import { ProductService } from "./demo/service/product.service";
import { LayoutService } from "./layout/service/app.layout.service";
import { APP_ROUTES } from "./app.routes";
import { ModuleConfigComponent } from "./config/module-config.component";

@NgModule({
  declarations: [
    AppMenuitemComponent,
    AppTopBarComponent,
    AppFooterComponent,
    AppMenuComponent,
    AppSidebarComponent,
    AppLayoutComponent,
    AppComponent,
    NotfoundComponent,
    ModuleConfigComponent
  ],
  imports: [
    RouterModule.forRoot(APP_ROUTES, {
      scrollPositionRestoration: 'enabled',
      anchorScrolling: 'enabled',
      onSameUrlNavigation: 'reload'
    }),
    BrowserModule,
    FormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    InputTextModule,
    SidebarModule,
    BadgeModule,
    RadioButtonModule,
    InputSwitchModule,
    RippleModule,
    RouterModule,
    AppConfigModule,
    CommonModule

  ],
  providers: [
    {provide: LocationStrategy, useClass: HashLocationStrategy},
    CountryService, CustomerService, EventService, IconService, NodeService,
    PhotoService, ProductService, LayoutService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
