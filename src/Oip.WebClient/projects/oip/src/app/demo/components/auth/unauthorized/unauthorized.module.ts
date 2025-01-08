import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UnauthorizedRoutingModule } from './unauthorized-routing.module';
import { UnauthorizedComponent } from './unauthorized.component';
import { ButtonModule } from 'primeng/button';
import { LogoComponent } from "oip-common";

@NgModule({
  imports: [
    CommonModule,
    UnauthorizedRoutingModule,
    ButtonModule,
    LogoComponent
  ],
  declarations: [UnauthorizedComponent]
})
export class UnauthorizedModule {
}
