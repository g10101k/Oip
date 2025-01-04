import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'weather-settings',
  templateUrl: './weather-settings.component.html'
})
export class WeatherSettingsComponent implements OnInit, OnDestroy {

  constructor() {
  }

  ngOnDestroy(): void {
  }

  ngOnInit(): void {

  }

  contentClick() {
  }
}
