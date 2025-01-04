import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { AuthConfigModule, MsgService, PutSecurityDto, SecurityDataService, SecurityService } from "common";
import { MultiSelectModule } from "primeng/multiselect";
import { TooltipModule } from "primeng/tooltip";
import { ButtonModule } from "primeng/button";
import { AuthModule } from "angular-auth-oidc-client";
import { FormsModule } from "@angular/forms";

@Component({
  selector: 'security',
  templateUrl: './security.component.html',
  imports: [
    MultiSelectModule,
    TooltipModule,
    FormsModule,
    ButtonModule
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
      console.log(result);
    }, error => this.msgService.error(error));
  }

  saveKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter' || $event.key === 'Space') {
      this.saveKeyDown(null);
    }
  }
}
