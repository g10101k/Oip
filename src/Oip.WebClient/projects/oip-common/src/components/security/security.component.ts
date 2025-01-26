import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { MultiSelectModule } from "primeng/multiselect";
import { TooltipModule } from "primeng/tooltip";
import { ButtonModule } from "primeng/button";
import { FormsModule } from "@angular/forms";
import { NgForOf } from "@angular/common";
import { MsgService } from "../../services/msg.service";
import { SecurityDataService } from "../../services/security-data.service";
import { PutSecurityDto } from "../../dtos/put-security.dto";

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
  @Input() id: number;
  @Input() controller: string;
  roles: string[] = [];

  constructor() {
  }

  ngOnDestroy(): void {
    // on destroy
  }

  ngOnInit(): void {
    if (!this.id) {
      this.msgService.error("Module id not passed!");
    }
    if (!this.controller) {
      this.msgService.error("Controller not passed!");
    }
    this.dataService.getSecurity(this.controller, this.id).then(result => {
      this.securityData = result;
    }, error => this.msgService.error(error));

    this.dataService.getRealmRoles().then(result => {
      this.roles = result;
    }, error => this.msgService.error(error));
  }

  saveClick() {
    let request: PutSecurityDto = { id: this.id, securities: this.securityData };
    this.dataService.saveSecurity(this.controller, request).then(result => {
      this.msgService.success('Saved security');
    }, error => this.msgService.error(error));
  }

  saveKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter' || $event.key === 'Space') {
      this.saveKeyDown(null);
    }
  }
}
