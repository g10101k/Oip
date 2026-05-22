import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { PrimeIcons } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { AddModuleInstanceDto, IntKeyValueDto } from '../../api/data-contracts';
import { MenuApi } from '../../api/menu.api';
import { MenuService } from '../../services/app.menu.service';
import { MsgService } from '../../services/msg.service';

type PrimeIconOption = {
  label: string;
  value: string;
};

@Component({
  selector: 'menu-item-create-dialog',
  standalone: true,
  imports: [ButtonModule, DialogModule, InputTextModule, SelectModule, FormsModule, TranslatePipe],
  template: `
    <p-dialog
      header="{{ 'menuItemCreateDialogComponent.header' | translate }}"
      [modal]="true"
      [style]="{ width: '40rem' }"
      [(visible)]="visible"
      (keydown.enter)="save()">
      @if (menuService.contextMenuItem) {
        <div class="flex items-center gap-4 mb-4 mt-1">
          <label class="font-semibold w-1/3" for="oip-menu-item-create-dialog-parent-input">
            {{ 'menuItemCreateDialogComponent.parentLabel' | translate }}
          </label>
          <input
            autocomplete="off"
            class="flex-auto"
            id="oip-menu-item-create-dialog-parent-input"
            pInputText
            readonly
            [ngModel]="menuService.contextMenuItem?.label" />
        </div>
      }
      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="oip-menu-item-create-label">
          {{ 'menuItemCreateDialogComponent.label' | translate }}
        </label>
        <input autocomplete="off" class="flex-auto" id="oip-menu-item-create-label" pInputText [(ngModel)]="label" />
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="oip-menu-item-create-module">
          {{ 'menuItemCreateDialogComponent.module' | translate }}
        </label>
        <p-select
          appendTo="body"
          class="flex-auto"
          id="oip-menu-item-create-module"
          optionLabel="value"
          optionValue="key"
          placeholder="{{ 'menuItemCreateDialogComponent.selectModule' | translate }}"
          [options]="modules"
          [(ngModel)]="selectModule" />
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="oip-menu-item-create-dialog-icon">
          {{ 'menuItemCreateDialogComponent.icon' | translate }}
        </label>
        <p-select
          appendTo="body"
          class="flex-auto"
          filterBy="label,value"
          id="oip-menu-item-create-dialog-icon"
          optionLabel="label"
          optionValue="value"
          scrollHeight="18rem"
          [filter]="true"
          [options]="iconOptions"
          [(ngModel)]="selectIcon">
          <ng-template let-icon pTemplate="selectedItem">
            <div class="flex items-center gap-2">
              <i [class]="icon.value"></i>
              <span>{{ icon.label }}</span>
            </div>
          </ng-template>
          <ng-template let-icon pTemplate="item">
            <div class="flex items-center gap-2">
              <i [class]="icon.value"></i>
              <span>{{ icon.label }}</span>
            </div>
          </ng-template>
        </p-select>
      </div>
      <div class="flex justify-end gap-2">
        <p-button
          id="oip-menu-item-create-cancel"
          label="{{ 'menuItemCreateDialogComponent.cancel' | translate }}"
          severity="secondary"
          [disabled]="saving"
          (click)="changeVisible()" />
        <p-button
          id="oip-menu-item-create-save"
          label="{{ 'menuItemCreateDialogComponent.save' | translate }}"
          [disabled]="!canSave"
          [loading]="saving"
          (click)="save()" />
      </div>
    </p-dialog>
  `
})
export class MenuItemCreateDialogComponent implements OnInit {
  protected readonly menuService = inject(MenuService);
  protected readonly menu = inject(MenuApi);
  private readonly msgService = inject(MsgService);
  @Input() visible!: boolean;
  @Output() visibleChange = new EventEmitter<boolean>();
  modules: IntKeyValueDto[] = [];
  iconOptions: PrimeIconOption[] = Object.values(PrimeIcons)
    .filter((icon): icon is string => typeof icon === 'string')
    .map((icon) => ({
      label: icon.replace('pi pi-', ''),
      value: icon
    }))
    .sort((left, right) => left.label.localeCompare(right.label));
  selectModule: any;
  label: string;
  selectIcon: string = 'pi pi-box';
  saving = false;

  get canSave(): boolean {
    return !this.saving && !!this.selectModule;
  }

  async ngOnInit() {
    this.modules = await this.menu.getModules();
  }

  changeVisible() {
    this.visible = !this.visible;
    this.visibleChange.emit(this.visible);
  }

  async save() {
    if (this.saving) {
      return;
    }

    if (!this.selectModule) {
      return;
    }

    const item: AddModuleInstanceDto = {
      moduleId: this.selectModule,
      label: this.label,
      icon: this.selectIcon,
      parentId: this.menuService.contextMenuItem?.moduleInstanceId
    };

    this.saving = true;
    try {
      await this.menuService.addModuleInstance(item);
      await this.menuService.loadMenu();
      this.hide();
    } catch (error) {
      this.msgService.errorFromException(error, 'Unexpected error');
    } finally {
      this.saving = false;
    }
  }

  hide() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  showDialog() {
    this.visible = true;
    this.visibleChange.emit(this.visible);
  }
}
