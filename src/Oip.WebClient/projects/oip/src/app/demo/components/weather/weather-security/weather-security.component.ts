import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { WeatherDataService } from "../weather-data.service";
import { MsgService, PutSecurityDto } from "common";

@Component({
  selector: 'weather-security',
  templateUrl: './weather-security.component.html'
})
export class WeatherSecurityComponent implements OnInit, OnDestroy {
  msgService = inject(MsgService);
  dataService = inject(WeatherDataService);
  securityData: any[];
  @Input() id: number | undefined = undefined;
  roles: string[] = ["admin", "user", "test"];

  constructor() {
  }

  ngOnDestroy(): void {
  }

  ngOnInit(): void {
    this.dataService.getSecurity(this.id).then(result => {
      this.securityData = result;
    }, error => this.msgService.error(error));
  }

  saveClick() {
    let request: PutSecurityDto = { id: this.id, securities: this.securityData };
    this.dataService.saveSecurity(request).then(result => {
      console.log(result);
    }, error => this.msgService.error(error));
  }

  saveKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter' || $event.key === 'Space' ) {
      this.saveKeyDown(null);
    }
  }
}
