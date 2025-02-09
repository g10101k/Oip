import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { DropdownModule } from "primeng/dropdown";
import { FormsModule } from "@angular/forms";
import { PrimeIcons } from "primeng/api";
import { AddModuleInstanceDto, MenuService } from "oip-common";
import { Select } from "primeng/select";
import { TranslatePipe } from "@ngx-translate/core";

@Component({
  selector: 'create-menu-item-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    DialogModule,
    InputTextModule,
    DropdownModule,
    FormsModule,
    Select,
    TranslatePipe
  ],
  template: `
    <p-dialog header="{{ 'createMenuItemDialogComponent.header' | translate }}" [modal]="true" [(visible)]="visible"
              [style]="{ width: '25rem' }">
      <div class="flex items-center gap-4 mb-4">
        <label for="menuLabel"
               class="font-semibold w-24">{{ 'createMenuItemDialogComponent.label' | translate }}</label>
        <input pInputText id="menuLabel" class="flex-auto" autocomplete="off" [(ngModel)]="label"/>
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label for="module" class="font-semibold w-24">{{ 'createMenuItemDialogComponent.module' | translate }}</label>
        <p-select
          id="module"
          class="flex-auto"
          [options]="modules"
          [(ngModel)]="selectModule"
          optionLabel="value"
          optionValue="key"
          placeholder="{{'createMenuItemDialogComponent.selectModule' | translate }}"/>
      </div>
      <div class="flex items-center gap-4 mb-4">
        <label for="icon" class="font-semibold w-24">{{ 'createMenuItemDialogComponent.icon' | translate }}</label>
        <input pInputText id="icon" class="flex-auto" [(ngModel)]="selectIcon"/>
      </div>
      <div class="flex justify-end gap-2">
        <p-button label="{{ 'createMenuItemDialogComponent.cancel' | translate }}"
                  severity="secondary"
                  (click)="changeVisible()"
                  (keydown)="changeVisible()"/>
        <p-button label="{{ 'createMenuItemDialogComponent.save' | translate }}"
                  (click)="save()"
                  (keydown)="save()"/>
      </div>
    </p-dialog>
  `
})
export class CreateMenuItemDialogComponent implements OnInit {

  menuService = inject(MenuService);
  @Input() visible!: boolean;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() contextItem: any;

  modules: any[] = [];
  selectModule: any;
  label: string;
  icons: string[] = [
    PrimeIcons.ALIGN_CENTER,
    PrimeIcons.ALIGN_LEFT,
    PrimeIcons.HOURGLASS,
    PrimeIcons.ALIGN_RIGHT,
    PrimeIcons.HOME
  ];
  selectIcon: string;

  ngOnInit(): void {
    this.menuService.getModules().then(data => {
      this.modules = data;
    });
  }

  changeVisible() {
    this.visible = !this.visible;
  }

  save() {
    let item: AddModuleInstanceDto = {
      moduleId: this.selectModule,
      label: this.label,
      icon: this.selectIcon,
      parentId: this.contextItem?.moduleInstanceId
    };
    this.menuService.addModuleInstance(item).then(() => {
      this.hide();
    });
  }

  cancel() {
    this.visibleChange.emit(true);
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
