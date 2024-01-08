import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'weather-security',
  templateUrl: './weather-security.component.html'
})
export class WeatherSecurityComponent implements OnInit, OnDestroy {

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
  }

  ngOnDestroy(): void {
  }

  ngOnInit(): void {

  }

  contentClick() {
  }
}
