import { Component, inject } from '@angular/core';
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { MenuService } from './../../services/app.menu.service';
import { InputTextModule } from "primeng/inputtext";

@Component({
  selector: 'lib-create-menu-item-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    DialogModule,
    InputTextModule
  ],
  templateUrl: './create-menu-item-dialog.component.html',
  styleUrl: './create-menu-item-dialog.component.css'
})
export class CreateMenuItemDialogComponent {
  menuService= inject(MenuService);
  visible: boolean;

  changeVisible(){
    this.visible = !this.visible;
  }

  showDialog() {
    // example
  }
}
