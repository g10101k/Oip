import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MenuService } from "../../services/app.menu.service";
import { LayoutService } from "../../services/app.layout.service";
import { MenuItemComponent } from '../menu-item/menu-item.component';
import { NgFor, NgIf } from '@angular/common';
import { ButtonModule } from "primeng/button";
import { SecurityService } from "../../services/security.service";
import { ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from "primeng/dialog";
import { MenuItem, MenuItemCommandEvent, PrimeIcons } from "primeng/api";
import { InputTextModule } from "primeng/inputtext";
import { CreateMenuItemDialogComponent } from "../create-menu-item-dialog/create-menu-item-dialog.component";
import { MsgService } from '../../services/msg.service';
import { InputSwitchModule } from 'primeng/inputswitch';
import { FormsModule } from "@angular/forms";


@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  standalone: true,
  imports: [NgFor, NgIf, MenuItemComponent, ButtonModule, ContextMenuModule, DialogModule, InputTextModule, CreateMenuItemDialogComponent, InputSwitchModule, FormsModule]
})
export class MenuComponent implements OnInit {
  readonly menuService = inject(MenuService);
  readonly layoutService = inject(LayoutService);
  readonly securityService = inject(SecurityService);
  readonly msgService = inject(MsgService);
  @ViewChild(CreateMenuItemDialogComponent) createMenuItemDialogComponent: CreateMenuItemDialogComponent;
  model: any[] = [];
  emptySpaceMenu: MenuItem[] = [
    { label: 'New', icon: PrimeIcons.PLUS, command: (event) => this.newClick(event) },
  ];
  adminMode: boolean = false;

  ngOnInit() {
    this.loadMenu();
  }

  private newClick(e: MenuItemCommandEvent) {
    this.createMenuItemDialogComponent.showDialog();
  }

  onSettingButtonClick() {
    this.loadMenu();
  }

  loadMenu() {
    if (this.adminMode) {
      this.menuService.getAdminMenu().then(
        menu => {
          this.model = menu;
        }, error => this.msgService.error(error));
    } else {
      this.menuService.getMenu().then(
        menu => {
          this.model = menu;
        }, error => this.msgService.error(error));
    }
  }
}
