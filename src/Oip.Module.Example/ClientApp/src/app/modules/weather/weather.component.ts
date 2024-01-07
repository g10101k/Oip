import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WeatherForecast } from '../../../dtos/weather.forecast'
import { TopBarItem, ModuleTopBarService } from 'shared-lib'
import { B } from "@fullcalendar/core/internal-common";

@Component({
  selector: 'weather',
  templateUrl: './weather.component.html'
})
export class WeatherComponent implements OnInit, OnDestroy {
  protected forecasts: WeatherForecast[] = [];
  protected activeIndex: number = 0;
  protected isContent: boolean = true;
  protected isSetting: boolean = false;
  protected isSecurity: boolean = false;
  private topBarItems: TopBarItem[] = [
    { id: 'content', icon: 'pi-box', caption: 'Content', },
    { id: 'settings', icon: 'pi-cog', caption: 'Settings', },
    { id: 'security', icon: 'pi-lock', caption: 'Security', click: this.securityClick },
  ];


  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, public topBarService: ModuleTopBarService) {
    http.get<WeatherForecast[]>(baseUrl + 'api/weatherforecast').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));
    this.topBarService.setTopBarItems(this.topBarItems);

  }

  ngOnDestroy(): void {
    this.topBarService.setTopBarItems([]);
  }

  ngOnInit(): void {
    this.topBarService.setTopBarItems(this.topBarItems);
  }

  securityClick() {
    console.log('securityClick');
  }
}
