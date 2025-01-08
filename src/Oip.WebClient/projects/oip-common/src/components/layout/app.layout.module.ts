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
import { AppLayoutComponent } from "./app.layout.component";
import { ButtonModule } from "primeng/button";
import { TabViewModule } from "primeng/tabview";
import { SharedModule } from "primeng/api";
import { MenuModule } from 'primeng/menu';
import { MenuItemComponent } from "../menu-item/menu-item.component";
import { NgForOf } from "@angular/common";
import { MenuComponent } from "../menu/menu.component";
import { SidebarComponent } from "../sidebar/sidebar.component";
import { TopBarComponent } from "../topbar/top-bar.component";
import { FooterComponent } from "../footer/footer.component";
import { LogoComponent } from "../logo/logo.component";

@NgModule({
  declarations: [
    MenuComponent,
    SidebarComponent,
    AppLayoutComponent,
    MenuItemComponent,
    TopBarComponent,
    FooterComponent
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
    LogoComponent,
  ],
  exports: [AppLayoutComponent, FooterComponent, TopBarComponent]
})
export class AppLayoutModule {
}
