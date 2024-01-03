import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { FlightsModule } from './flights/flights.module';
import { APP_ROUTES } from './app.routes';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  imports: [
    BrowserModule,
    HttpClientModule,
    FlightsModule,
    RouterModule.forRoot(APP_ROUTES),
  ],
  declarations: [
    HomeComponent,
    AppComponent,
  ],
  providers: [{ provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }],
  bootstrap: [
      AppComponent
  ]
})
export class AppModule { }

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}
