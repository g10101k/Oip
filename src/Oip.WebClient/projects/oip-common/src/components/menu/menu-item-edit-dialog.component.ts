import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { PrimeIcons } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { MenuService } from '../../services/app.menu.service';
import { MsgService } from '../../services/msg.service';
import { TranslatePipe } from '@ngx-translate/core';
import { MultiSelectModule } from 'primeng/multiselect';
import { EditModuleInstanceDto } from '../../api/data-contracts';
import { SecurityApi } from '../../api/security.api';

type PrimeIconOption = {
  label: string;
  value: string;
};

@Component({
  imports: [ButtonModule, DialogModule, InputTextModule, SelectModule, FormsModule, TranslatePipe, MultiSelectModule],
  selector: 'menu-item-edit-dialog',
  standalone: true,
  template: `
    <p-dialog
      header="{{ 'menuItemEditDialogComponent.header' | translate }}"
      [modal]="true"
      [style]="{ width: '40rem' }"
      [(visible)]="visible"
      (keydown.enter)="save()">
      <div class="flex items-center gap-4 mb-4 mt-1">
        <label class="font-semibold w-1/3" for="oip-menu-item-edit-dialog-menu-input">
          {{ 'menuItemEditDialogComponent.label' | translate }}
        </label>
        <input
          autocomplete="off"
          class="flex-auto"
          id="oip-menu-item-edit-dialog-menu-input"
          pInputText
          [(ngModel)]="item.label" />
      </div>

      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="oip-menu-item-edit-dialog-icon">
          {{ 'menuItemEditDialogComponent.icon' | translate }}
        </label>
        <p-select
          appendTo="body"
          class="flex-auto"
          filterBy="label,value"
          id="oip-menu-item-edit-dialog-icon"
          optionLabel="label"
          optionValue="value"
          scrollHeight="18rem"
          [filter]="true"
          [options]="iconOptions"
          [(ngModel)]="item.icon">
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

      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="security">
          {{ 'menuItemEditDialogComponent.security' | translate }}
        </label>
        <p-multiSelect
          appendTo="body"
          class="flex-auto"
          id="oip-menu-item-edit-dialog-roles-multi-select"
          placeholder="Select roles"
          [maxSelectedLabels]="10"
          [options]="roles"
          [(ngModel)]="item.viewRoles" />
      </div>

      <div class="flex justify-end gap-2">
        <p-button
          id="oip-menu-item-edit-dialog-cancel-edit-button"
          label="{{ 'menuItemEditDialogComponent.cancel' | translate }}"
          severity="secondary"
          [disabled]="saving"
          (click)="changeVisible()" />
        <p-button
          id="oip-menu-item-edit-dialog-save-edit-button"
          label="{{ 'menuItemEditDialogComponent.save' | translate }}"
          [disabled]="saving"
          [loading]="saving"
          (click)="save()" />
      </div>
    </p-dialog>
  `
})
export class MenuItemEditDialogComponent {
  private readonly menuService = inject(MenuService);
  private readonly securityApi = inject(SecurityApi);
  private readonly msgService = inject(MsgService);

  @Input() visible!: boolean;
  @Output() visibleChange = new EventEmitter<boolean>();

  modules: any[] = [];
  roles: string[] = [];
  iconOptions: PrimeIconOption[] = Object.values(PrimeIcons)
    .filter((icon): icon is string => typeof icon === 'string')
    .map((icon) => ({
      label: icon.replace('pi pi-', ''),
      value: icon
    }))
    .sort((left, right) => left.label.localeCompare(right.label));
  item: EditModuleInstanceDto = {
    icon: '',
    label: '',
    viewRoles: [''],
    moduleId: 0,
    moduleInstanceId: 0,
    parentId: 0
  };
  saving = false;

  changeVisible() {
    this.visible = !this.visible;
    this.visibleChange.emit(this.visible);
  }

  async save() {
    if (this.saving) {
      return;
    }

    this.saving = true;
    try {
      await this.menuService.editModuleInstance(this.item);
      await this.menuService.loadMenu();
      this.hide();
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.saving = false;
    }
  }

  hide() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  async showDialog() {
    this.item = {
      moduleInstanceId: this.menuService.contextMenuItem?.moduleInstanceId,
      moduleId: this.menuService.contextMenuItem?.moduleId,
      parentId: this.menuService.contextMenuItem?.parentId,
      label: this.menuService.contextMenuItem?.label,
      icon: this.menuService.contextMenuItem?.icon,
      viewRoles: this.menuService.contextMenuItem?.securities
    };

    this.roles = await this.securityApi.getRealmRoles();
    this.menuService.getModules().then((data) => {
      this.modules = data;
    });

    this.visible = true;
    this.visibleChange.emit(this.visible);
  }
}
