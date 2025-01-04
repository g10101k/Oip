import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { MsgService, PutSecurityDto, SecurityDataService } from "common";
import { MultiSelectModule } from "primeng/multiselect";
import { TooltipModule } from "primeng/tooltip";
import { ButtonModule } from "primeng/button";
import { FormsModule } from "@angular/forms";
import { NgForOf } from "@angular/common";
import { msg } from "ng-packagr/lib/utils/log";

@Component({
  selector: 'security',
  templateUrl: './security.component.html',
  imports: [
    MultiSelectModule,
    TooltipModule,
    FormsModule,
    ButtonModule,
    NgForOf
  ],
  standalone: true
})
export class SecurityComponent implements OnInit, OnDestroy {
  msgService = inject(MsgService);
  dataService = inject(SecurityDataService);
  securityData: any[];
  @Input() id: number | undefined = undefined;
  roles: string[] = [];

  constructor() {
  }

  ngOnDestroy(): void {
  }

  ngOnInit(): void {
    this.dataService.getSecurity(this.id).then(result => {
      this.securityData = result;
    }, error => this.msgService.error(error));

    this.dataService.getRealmRoles().then(result => {
      this.roles = result;
    }, error => this.msgService.error(error));
  }

  saveClick() {
    let request: PutSecurityDto = { id: this.id, securities: this.securityData };
    this.dataService.saveSecurity(request).then(result => {
      this.msgService.success('Saved security');
    }, error => this.msgService.error(error));
  }

  saveKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter' || $event.key === 'Space') {
      this.saveKeyDown(null);
    }
  }
}
