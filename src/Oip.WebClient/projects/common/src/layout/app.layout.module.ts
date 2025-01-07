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
import { AppMenuitemComponent } from './app.menuitem.component';
import { RouterModule } from '@angular/router';
import { TopBarComponent, FooterComponent, MenuComponent, ConfigComponent } from 'oip/common';
import { AppSidebarComponent } from "./app.sidebar.component";
import { AppLayoutComponent } from "./app.layout.component";
import { ButtonModule } from "primeng/button";
import { TabViewModule } from "primeng/tabview";
import { SharedModule } from "primeng/api";
import { MenuModule } from 'primeng/menu';

@NgModule({
  declarations: [
    AppMenuitemComponent,
    TopBarComponent,
    MenuComponent,
    AppSidebarComponent,
    AppLayoutComponent,
  ],
  imports: [
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
    ButtonModule,
    TabViewModule,
    SharedModule,
    TabViewModule,
    MenuModule,
    FooterComponent,
    ConfigComponent
  ],
  exports: [AppLayoutComponent]
})
export class AppLayoutModule {
}
