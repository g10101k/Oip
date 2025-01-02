import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UnauthorizedRoutingModule } from './unauthorized-routing.module';
import { UnauthorizedComponent } from './unauthorized.component';
import { ButtonModule } from 'primeng/button';

@NgModule({
    imports: [
        CommonModule,
        UnauthorizedRoutingModule,
        ButtonModule,
        UnauthorizedComponent
    ]
})
export class UnauthorizedModule { }
