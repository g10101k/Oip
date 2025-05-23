import { Component, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { MultiSelectModule } from "primeng/multiselect";
import { TooltipModule } from "primeng/tooltip";
import { ButtonModule } from "primeng/button";
import { FormsModule } from "@angular/forms";
import { MsgService } from "./../services/msg.service";
import { SecurityDataService } from "./../services/security-data.service";
import { PutSecurityDto } from "./../dtos/put-security.dto";
import { Fluid } from "primeng/fluid";
import { TranslatePipe, TranslateService } from "@ngx-translate/core";

@Component({
  selector: 'security',
  template: `
    <p-fluid>
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'securityComponent.security' | translate }}</div>
            @for (item of securityData; track item.name) {
              <div class="flex flex-col gap-2">
                <label htmlFor="{{ item.name }} ">{{ item.name }} <span pTooltip="{{ item.description }}"
                                                                        tooltipPosition="right"
                                                                        class="pi pi-question-circle"></span>
                </label>
                <p-multiSelect [options]="roles"
                               [maxSelectedLabels]="10"
                               [(ngModel)]="item.roles"
                               placeholder="Select roles"/>
              </div>
            }
            <div class="flex justify-content-end flex-wrap">
              <p-button label="{{ 'securityComponent.save' | translate }}"
                        icon="pi pi-save"
                        (click)="saveClick()"
                        (keydown)="saveKeyDown($event)"/>
            </div>
          </div>
        </div>
      </div>
    </p-fluid>
  `,
  imports: [
    MultiSelectModule,
    TooltipModule,
    FormsModule,
    ButtonModule,
    Fluid,
    TranslatePipe,
  ],
  standalone: true
})
export class SecurityComponent implements OnInit, OnDestroy {
  private readonly msgService = inject(MsgService);
  private readonly dataService = inject(SecurityDataService);
  private readonly translateService = inject(TranslateService);
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
      this.msgService.success(this.translateService.instant('securityComponent.savedSecurity'));
    }, error => this.msgService.error(error));
  }

  saveKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter' || $event.key === 'Space') {
      this.saveKeyDown(null);
    }
  }
}
