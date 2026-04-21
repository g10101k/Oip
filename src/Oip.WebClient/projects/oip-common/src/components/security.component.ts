import { Component, inject, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { MultiSelectModule } from 'primeng/multiselect';
import { TooltipModule } from 'primeng/tooltip';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { MsgService } from './../services/msg.service';
import { SecurityDataService } from './../services/security-data.service';
import { PutSecurityDto } from './../dtos/put-security.dto';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'security',
  template: `
    <div class="flex flex-col md:flex-row gap-8">
      <div class="md:w-1/2">
        <div class="card flex flex-col gap-4">
          <div class="font-semibold text-xl">
            {{ 'securityComponent.security' | translate }}
          </div>
          @for (item of securityData; track item.name) {
            <div class="flex flex-col gap-2">
              <label htmlFor="oip-security-multiselect-{{ item.name }}">
                {{ item.name }}
                <span class="pi pi-question-circle" pTooltip="{{ item.description }}" tooltipPosition="right"></span>
              </label>
              <p-multiSelect
                id="oip-security-multiselect-{{ item.name }}"
                placeholder="{{ 'securityComponent.selectRoles' | translate }}"
                [maxSelectedLabels]="10"
                [options]="roles"
                [(ngModel)]="item.roles" />
            </div>
          }
          <div class="flex justify-content-end flex-wrap">
            <p-button
              icon="pi pi-save"
              id="oip-security-save-button"
              label="{{ 'securityComponent.save' | translate }}"
              (click)="saveClick()"
              (keydown)="saveKeyDown($event)" />
          </div>
        </div>
      </div>
    </div>
  `,
  imports: [MultiSelectModule, TooltipModule, FormsModule, ButtonModule, TranslatePipe],
  standalone: true
})
export class SecurityComponent implements OnChanges, OnInit, OnDestroy {
  private readonly msgService = inject(MsgService);
  private readonly dataService = inject(SecurityDataService);
  private readonly translateService = inject(TranslateService);
  private securityLoadToken = 0;

  securityData: any[] = [];
  @Input() id?: number;
  @Input() controller?: string;
  roles: string[] = [];

  ngOnDestroy(): void {
    // on destroy
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['id'] || changes['controller']) {
      void this.loadSecurity();
    }
  }

  ngOnInit(): void {
    this.dataService.getRealmRoles().then(
      (result) => {
        this.roles = result;
      },
      (error) => this.msgService.error(error)
    );
  }

  saveClick() {
    if (this.id == null) {
      this.msgService.error('Module id not passed!');
      return;
    }
    if (!this.controller) {
      this.msgService.error('Controller not passed!');
      return;
    }

    const request: PutSecurityDto = {
      id: this.id,
      securities: this.securityData
    };
    this.dataService.saveSecurity(this.controller, request).then(
      (result) => {
        this.msgService.success(this.translateService.instant('securityComponent.savedSecurity'));
      },
      (error) => this.msgService.error(error)
    );
  }

  saveKeyDown($event: KeyboardEvent) {
    if ($event.key === 'Enter' || $event.key === 'Space') {
      this.saveClick();
    }
  }

  private async loadSecurity(): Promise<void> {
    const loadToken = ++this.securityLoadToken;
    const controller = this.controller;
    const id = this.id;

    this.securityData = [];

    if (!controller || id == null) {
      return;
    }

    try {
      const result = await this.dataService.getSecurity(controller, id);
      if (loadToken === this.securityLoadToken) {
        this.securityData = result;
      }
    } catch (error) {
      if (loadToken === this.securityLoadToken) {
        this.msgService.error(error);
      }
    }
  }
}
