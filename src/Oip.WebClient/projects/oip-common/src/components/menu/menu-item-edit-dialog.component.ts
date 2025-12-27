import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { MenuService } from '../../services/app.menu.service';
import { SecurityDataService } from '../../services/security-data.service';
import { TranslatePipe } from '@ngx-translate/core';
import { EditModuleInstanceDto } from '../../dtos/edit-module-instance.dto';
import { MultiSelectModule } from 'primeng/multiselect';

@Component({
  imports: [ButtonModule, DialogModule, InputTextModule, FormsModule, TranslatePipe, MultiSelectModule],
  selector: 'menu-item-edit-dialog',
  standalone: true,
  template: `
    <p-dialog
      header="{{ 'menuItemEditDialogComponent.header' | translate }}"
      [modal]="true"
      [style]="{ width: '40rem' }"
      [(visible)]="visible">
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
        <i class="{{ item.icon }}"></i>
        <input class="flex-auto" id="oip-menu-item-edit-dialog-icon" pInputText [(ngModel)]="item.icon" />
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
          (click)="changeVisible()"
          (keydown)="changeVisible()" />
        <p-button
          id="oip-menu-item-edit-dialog-save-edit-button"
          label="{{ 'menuItemEditDialogComponent.save' | translate }}"
          (click)="save()"
          (keydown)="save()" />
      </div>
    </p-dialog>
  `
})
export class MenuItemEditDialogComponent {
  private readonly menuService = inject(MenuService);
  private readonly securityDataService = inject(SecurityDataService);

  @Input() visible!: boolean;
  @Output() visibleChange = new EventEmitter<boolean>();

  modules: any[] = [];
  roles: string[] = [];
  item: EditModuleInstanceDto = {
    icon: '',
    label: '',
    viewRoles: [''],
    moduleId: 0,
    moduleInstanceId: 0,
    parentId: 0
  };

  changeVisible() {
    this.visible = !this.visible;
    this.visibleChange.emit(this.visible);
  }

  async save() {
    await this.menuService.editModuleInstance(this.item);
    await this.menuService.loadMenu();
    this.hide();
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

    this.roles = await this.securityDataService.getRealmRoles();
    this.menuService.getModules().then((data) => {
      this.modules = data;
    });

    this.visible = true;
    this.visibleChange.emit(this.visible);
  }
}
