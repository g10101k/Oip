import { NgModule } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { AuthLibModule } from 'auth-lib';
import { SharedLibModule } from 'shared-lib';
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TabViewModule } from "primeng/tabview";
import { OtherModuleComponent } from "./other-module.component";


@NgModule({
  imports: [
    CommonModule,
    AuthLibModule,
    SharedLibModule,
    CurrencyPipe,
    DatePipe,
    TableModule,
    TagModule,
    TabViewModule,
    RouterModule.forChild([
      {
        path: '',
        component: OtherModuleComponent
      },
      {
        path: 'other-module',
        component: OtherModuleComponent
      }
    ]),
  ],
  declarations: [
    OtherModuleComponent
  ]
})

export class OtherModule {
}
