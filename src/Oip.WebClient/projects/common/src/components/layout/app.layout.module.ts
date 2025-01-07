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
import { RouterModule } from '@angular/router';
import { TopBarComponent, FooterComponent, MenuComponent, SidebarComponent } from 'oip/common';
import { AppLayoutComponent } from "./app.layout.component";
import { ButtonModule } from "primeng/button";
import { TabViewModule } from "primeng/tabview";
import { SharedModule } from "primeng/api";
import { MenuModule } from 'primeng/menu';
import { MenuItemComponent } from "../menu-item/menu-item.component";
import { NgForOf } from "@angular/common";

@NgModule({
  declarations: [
    MenuComponent,
    SidebarComponent,
    AppLayoutComponent,
    MenuItemComponent,
    TopBarComponent,
    FooterComponent,

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
    NgForOf,
    NgForOf,
  ],
  exports: [AppLayoutComponent, FooterComponent, TopBarComponent]
})
export class AppLayoutModule {
}
