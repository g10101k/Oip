import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { DropdownModule } from "primeng/dropdown";
import { FormsModule } from "@angular/forms";
import { MenuService, SecurityDataService } from "oip-common";
import { TranslatePipe } from "@ngx-translate/core";
import { EditModuleInstanceDto } from "../../dtos/edit-module-instance.dto";

@Component({
  imports: [
    ButtonModule,
    DialogModule,
    InputTextModule,
    DropdownModule,
    FormsModule,
    TranslatePipe
  ],
  selector: 'menu-item-edit-dialog',
  standalone: true,
  template: `
    <p-dialog header="{{ 'menuItemEditDialogComponent.header' | translate }}" [modal]="true" [(visible)]="visible"
              [style]="{ width: '40rem' }">
      <div class="flex items-center gap-4 mb-4 mt-1">
        <label for="menuLabel"
               class="font-semibold w-1/3">{{ 'menuItemEditDialogComponent.label' | translate }}</label>
        <input pInputText id="menuLabel" class="flex-auto" autocomplete="off" [(ngModel)]="item.label"/>
      </div>

      <div class="flex items-center gap-4 mb-4">
        <label for="icon" class="font-semibold w-1/3">{{ 'menuItemEditDialogComponent.icon' | translate }}
        </label>
        <i class="{{item.icon}}"></i>
        <input pInputText id="icon" class="flex-auto" [(ngModel)]="item.icon"/>
      </div>

      <div class="flex justify-end gap-2">
        <p-button label="{{ 'menuItemEditDialogComponent.cancel' | translate }}"
                  severity="secondary"
                  (click)="changeVisible()"
                  (keydown)="changeVisible()"/>
        <p-button label="{{ 'menuItemEditDialogComponent.save' | translate }}"
                  (click)="save()"
                  (keydown)="save()"/>
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
  item: EditModuleInstanceDto = { icon: "", label: "", moduleId: 0, moduleInstanceId: 0, parentId: 0 };

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

  showDialog() {
    this.item = {
      moduleInstanceId: this.menuService.contextMenuItem?.moduleInstanceId,
      moduleId: this.menuService.contextMenuItem?.moduleId,
      parentId: this.menuService.contextMenuItem?.parentId,
      label: this.menuService.contextMenuItem?.label,
      icon: this.menuService.contextMenuItem?.icon
    };

    this.securityDataService.getRealmRoles().then((roles) => {
      this.roles = roles;
    });
    this.menuService.getModules().then(data => {
      this.modules = data;
    });

    this.visible = true;
    this.visibleChange.emit(this.visible);
  }
}
