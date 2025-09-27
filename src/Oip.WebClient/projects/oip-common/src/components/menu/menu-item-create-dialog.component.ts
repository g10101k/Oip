import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { AddModuleInstanceDto, MenuService } from 'oip-common';
import { Select } from 'primeng/select';
import { TranslatePipe } from '@ngx-translate/core';
import { NgIf } from '@angular/common';

@Component({
  selector: 'menu-item-create-dialog',
  standalone: true,
  imports: [ButtonModule, DialogModule, InputTextModule, SelectModule, FormsModule, Select, TranslatePipe, NgIf],
  template: `
    <p-dialog
      header="{{ 'menuItemCreateDialogComponent.header' | translate }}"
      [modal]="true"
      [style]="{ width: '40rem' }"
      [(visible)]="visible">
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
            [ngModel]="menuService.contextMenuItem?.label"/>
        </div>
      }
      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="oip-menu-item-create-label">
          {{ 'menuItemCreateDialogComponent.label' | translate }}
        </label>
        <input autocomplete="off" class="flex-auto" id="oip-menu-item-create-label" pInputText [(ngModel)]="label"/>
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
          [(ngModel)]="selectModule"/>
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label class="font-semibold w-1/3" for="oip-menu-item-create-dialog-icon">
          {{ 'menuItemCreateDialogComponent.icon' | translate }}
        </label>
        <i class="{{ selectIcon }}"></i>
        <input class="flex-auto" id="oip-menu-item-create-dialog-icon" pInputText [(ngModel)]="selectIcon"/>
      </div>
      <div class="flex justify-end gap-2">
        <p-button
          id="oip-menu-item-create-cancel"
          label="{{ 'menuItemCreateDialogComponent.cancel' | translate }}"
          severity="secondary"
          (click)="changeVisible()"
          (keydown)="changeVisible()"/>
        <p-button
          id="oip-menu-item-create-save"
          label="{{ 'menuItemCreateDialogComponent.save' | translate }}"
          (click)="save()"
          (keydown)="save()"/>
      </div>
    </p-dialog>
  `
})
export class MenuItemCreateDialogComponent implements OnInit {
  menuService = inject(MenuService);
  @Input() visible!: boolean;
  @Output() visibleChange = new EventEmitter<boolean>();
  modules: any[] = [];
  selectModule: any;
  label: string;
  selectIcon: string = 'pi pi-box';

  ngOnInit(): void {
    this.menuService.getModules().then((data) => {
      this.modules = data;
    });
  }

  changeVisible() {
    this.visible = !this.visible;
    this.visibleChange.emit(this.visible);
  }

  async save() {
    const item: AddModuleInstanceDto = {
      moduleId: this.selectModule,
      label: this.label,
      icon: this.selectIcon,
      parentId: this.menuService.contextMenuItem?.moduleInstanceId
    };
    await this.menuService.addModuleInstance(item);
    await this.menuService.loadMenu();
    this.hide();
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
