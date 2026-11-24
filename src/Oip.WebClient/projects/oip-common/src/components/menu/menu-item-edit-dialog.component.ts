import { Component, EventEmitter, inject, Input, Output, ViewChild } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { PrimeIcons } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { MenuService } from '../../services/app.menu.service';
import { MsgService } from '../../services/msg.service';
import { TranslatePipe } from '@ngx-translate/core';
import { EditModuleInstanceDto } from '../../api/data-contracts';
import { AccessRightDto } from '../../dtos/object-acl.dto';
import { ObjectAclEditorComponent } from '../object-acl-editor/object-acl-editor.component';
import { MODULE_ACCESS_CONTROL_API, MODULE_INSTANCE_OBJECT_TYPE } from '../security/security.component';

type PrimeIconOption = {
  label: string;
  value: string;
};

@Component({
  imports: [
    ButtonModule,
    DialogModule,
    InputTextModule,
    SelectModule,
    FormsModule,
    TranslatePipe,
    ObjectAclEditorComponent
  ],
  selector: 'menu-item-edit-dialog',
  standalone: true,
  template: `
    <p-dialog
      header="{{ 'menuItemEditDialogComponent.header' | translate }}"
      [modal]="true"
      [breakpoints]="{ '960px': '95vw' }"
      [style]="{ width: '56rem' }"
      [(visible)]="visible"
      (keydown.enter)="onEnter($event)">
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
        <label class="font-semibold w-1/3" for="oip-menu-item-edit-dialog-module">
          {{ 'menuItemEditDialogComponent.module' | translate }}
        </label>
        <input
          autocomplete="off"
          class="flex-auto"
          disabled
          id="oip-menu-item-edit-dialog-module"
          pInputText
          [ngModel]="moduleName" />
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

      @if (visible && item.moduleInstanceId) {
        <div class="flex flex-col gap-2 mb-4" id="oip-menu-item-edit-dialog-access">
          <div class="font-semibold">{{ 'menuItemEditDialogComponent.security' | translate }}</div>
          <oip-object-acl-editor
            #aclEditor
            [apiBase]="accessControlApi"
            [objectId]="item.moduleInstanceId"
            [objectType]="moduleInstanceType"
            [rights]="menuRights"
            [showSaveButton]="false" />
        </div>
      }

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
  private readonly msgService = inject(MsgService);

  @ViewChild('aclEditor') aclEditor?: ObjectAclEditorComponent;

  protected readonly accessControlApi = MODULE_ACCESS_CONTROL_API;
  protected readonly moduleInstanceType = MODULE_INSTANCE_OBJECT_TYPE;
  /** The menu item controls who sees the item; the other rights are set on the security tab of the module. */
  protected readonly menuRights: AccessRightDto[] = [{ code: 'read' }];

  @Input() visible!: boolean;
  @Output() visibleChange = new EventEmitter<boolean>();

  moduleName = '';
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
      // The rights are saved first: when they are rejected the dialog stays open with the user's changes.
      if (this.aclEditor && !(await this.aclEditor.save())) {
        return;
      }
      await this.menuService.editModuleInstance({ ...this.item, viewRoles: null });
      await this.menuService.loadMenu();
      this.hide();
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.saving = false;
    }
  }

  /** Enter saves the dialog, except while choosing a subject in the rights editor. */
  onEnter(event: Event) {
    if ((event.target as HTMLElement | null)?.closest('oip-object-acl-editor')) {
      return;
    }
    void this.save();
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
      icon: this.menuService.contextMenuItem?.icon
    };

    const modules = await this.menuService.getModules();
    this.moduleName = modules.find((module) => module.key === this.item.moduleId)?.value ?? '';

    this.visible = true;
    this.visibleChange.emit(this.visible);
  }
}
