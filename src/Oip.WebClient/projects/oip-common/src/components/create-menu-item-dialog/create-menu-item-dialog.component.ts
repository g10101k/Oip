import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { MenuService } from './../../services/app.menu.service';
import { InputTextModule } from "primeng/inputtext";
import { DropdownModule } from "primeng/dropdown";
import { FormsModule } from "@angular/forms";
import { PrimeIcons } from "primeng/api";
import { AddModuleInstanceDto } from "../../dtos/add-module-instance.dto";
@Component({
  selector: 'create-menu-item-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    DialogModule,
    InputTextModule,
    DropdownModule,
    FormsModule
  ],
  templateUrl: './create-menu-item-dialog.component.html'
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

  hide(){
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  showDialog() {
    this.visible = true;
    this.visibleChange.emit(this.visible);
  }
}
