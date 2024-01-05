import {NgModule} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import {FlightsSearchComponent} from './flights-search/flights-search.component';
import {RouterModule} from '@angular/router';
import {FLIGHTS_ROUTES} from './flights.routes';
import {AuthLibModule} from 'auth-lib';
import {SharedLibModule} from 'shared-lib';
import {Implement} from "@angular/cli/lib/config/workspace-schema";


@NgModule({
  imports: [
    CommonModule,
    AuthLibModule,
    SharedLibModule,
    RouterModule.forChild(FLIGHTS_ROUTES),
    CurrencyPipe,
    DatePipe,

  ],
  declarations: [
    FlightsSearchComponent
  ]
})
export class FlightsModule {
}
