import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StyleClassModule } from 'primeng/styleclass';
import { DividerModule } from 'primeng/divider';
import { ChartModule } from 'primeng/chart';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { NotfoundComponent } from "./notfound.component";
import { RouterModule } from "@angular/router";

@NgModule({
    imports: [
        CommonModule,
        DividerModule,
        StyleClassModule,
        ChartModule,
        PanelModule,
        ButtonModule,
        RouterModule.forChild([
            { path: '', component: NotfoundComponent }
        ]),
        NotfoundComponent
    ]
})

export class NotfoundModule {
}
