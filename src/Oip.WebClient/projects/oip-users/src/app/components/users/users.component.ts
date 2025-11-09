import { Component, OnDestroy, OnInit } from '@angular/core';
import { BaseModuleComponent, NoSettingsDto, SecurityComponent } from 'oip-common';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'users',
  templateUrl: './users.component.html',
  providers: [ConfirmationService],
  imports: [
    SecurityComponent
  ]
})
export class UserComponent extends BaseModuleComponent<NoSettingsDto, NoSettingsDto> implements OnInit, OnDestroy {
  async ngOnInit() {
    await super.ngOnInit();
  }
}
