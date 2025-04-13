import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { DropdownModule } from "primeng/dropdown";
import { FormsModule } from "@angular/forms";
import { AddModuleInstanceDto, MenuService } from "oip-common";
import { Select } from "primeng/select";
import { TranslatePipe } from "@ngx-translate/core";
import { NgIf } from "@angular/common";

@Component({
  selector: 'menu-item-create-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    DialogModule,
    InputTextModule,
    DropdownModule,
    FormsModule,
    Select,
    TranslatePipe,
    NgIf
  ],
  template: `
    <p-dialog header="{{ 'menuItemCreateDialogComponent.header' | translate }}" [modal]="true" [(visible)]="visible"
              [style]="{ width: '40rem',  height: '30rem'  }">
      <div class="flex items-center gap-4 mb-4 mt-1" *ngIf="menuService.contextMenuItem">
        <label for="parent"
               class="font-semibold w-1/3">{{ 'menuItemCreateDialogComponent.parentLabel' | translate }}</label>
        <input pInputText id="parent" class="flex-auto" autocomplete="off" readonly
               [ngModel]="menuService.contextMenuItem?.label"/>
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label for="menuLabel"
               class="font-semibold w-1/3">{{ 'menuItemCreateDialogComponent.label' | translate }}</label>
        <input pInputText id="menuLabel" class="flex-auto" autocomplete="off" [(ngModel)]="label"/>
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label for="module" class="font-semibold w-1/3">{{ 'menuItemCreateDialogComponent.module' | translate }}</label>
        <p-select
            id="module"
            class="flex-auto"
            [options]="modules"
            [(ngModel)]="selectModule"
            optionLabel="value"
            optionValue="key"
            placeholder="{{'menuItemCreateDialogComponent.selectModule' | translate }}"/>
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label for="icon" class="font-semibold w-1/3">{{ 'menuItemCreateDialogComponent.icon' | translate }}
        </label>
        <i class="{{selectIcon}}"></i>
        <input pInputText id="icon" class="flex-auto" [(ngModel)]="selectIcon"/>
      </div>
      <div class="flex justify-end gap-2">
        <p-button label="{{ 'menuItemCreateDialogComponent.cancel' | translate }}"
                  severity="secondary"
                  (click)="changeVisible()"
                  (keydown)="changeVisible()"/>
        <p-button label="{{ 'menuItemCreateDialogComponent.save' | translate }}"
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
    this.menuService.getModules().then(data => {
      this.modules = data;
    });
  }

  changeVisible() {
    this.visible = !this.visible;
    this.visibleChange.emit(this.visible);
  }

  async save() {
    let item: AddModuleInstanceDto = {
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
