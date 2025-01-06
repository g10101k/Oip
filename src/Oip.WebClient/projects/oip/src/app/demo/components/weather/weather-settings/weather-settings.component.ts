import { Component, Input, OnDestroy, OnInit, Output, EventEmitter } from '@angular/core';
import { WeatherSettingsDto } from "../dtos/weather-settings.dto";

@Component({
  selector: 'weather-settings',
  templateUrl: './weather-settings.component.html',
})
export class WeatherSettingsComponent implements OnInit, OnDestroy {

  @Input() settings: WeatherSettingsDto;
  @Output() settingsChange = new EventEmitter<WeatherSettingsDto>();

  constructor() {
  }

  ngOnDestroy(): void {
  }

  ngOnInit(): void {

  }

  saveButtonClick($event: MouseEvent) {
    this.settingsChange.emit(this.settings);
  }
}
