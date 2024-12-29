import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UnauthorizedRoutingModule } from './unauthorized-routing.module';
import { UnauthorizedComponent } from './unauthorized.component';
import { ButtonModule } from 'primeng/button';

@NgModule({
    imports: [
        CommonModule,
        UnauthorizedRoutingModule,
        ButtonModule
    ],
    declarations: [UnauthorizedComponent]
})
export class UnauthorizedModule { }
