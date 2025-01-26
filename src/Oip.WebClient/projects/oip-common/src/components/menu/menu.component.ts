import { Component, inject, OnInit, ViewChild, WritableSignal } from '@angular/core';
import { MenuService } from "../../services/app.menu.service";
import { LayoutService } from "../../services/app.layout.service";
import { MenuItemComponent } from '../menu-item/menu-item.component';
import { NgFor, NgIf } from '@angular/common';
import { ButtonModule } from "primeng/button";
import { SecurityService } from "../../services/security.service";
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from "primeng/dialog";
import { MenuItem, MenuItemCommandEvent, PrimeIcons } from "primeng/api";
import { InputTextModule } from "primeng/inputtext";
import { CreateMenuItemDialogComponent } from "../create-menu-item-dialog/create-menu-item-dialog.component";
import { MsgService } from '../../services/msg.service';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  standalone: true,
  imports: [NgFor, NgIf, MenuItemComponent, ButtonModule, ContextMenuModule, DialogModule, InputTextModule, CreateMenuItemDialogComponent]
})
export class MenuComponent implements OnInit {
  readonly menuService = inject(MenuService);
  readonly layoutService = inject(LayoutService);
  readonly securityService = inject(SecurityService);
  readonly msgService = inject(MsgService);

  @ViewChild(CreateMenuItemDialogComponent) createMenuItemDialogComponent: CreateMenuItemDialogComponent;
  @ViewChild('itemContextMenu') itemContextMenu: ContextMenu;


  model: any[] = [];
  emptySpaceMenu: MenuItem[] | undefined;

  newItemDialogVisible: boolean | WritableSignal<boolean>;

  ngOnInit() {
    this.menuService.getMenu().then(
      menu => {
        this.model = menu;
      }
    );
    this.emptySpaceMenu = [
      { label: 'New', icon: PrimeIcons.PLUS, command: (event) => this.newClick(event) },
      { label: 'Rename', icon: PrimeIcons.FILE_EDIT }
    ];
  }

  private newClick(e: MenuItemCommandEvent) {
    this.createMenuItemDialogComponent.showDialog();
  }

  onSettingButtonClick($event: MouseEvent) {
    this.menuService.getAdminMenu().then(
      menu => {
        this.model = menu;
      }, error => this.msgService.error(error));
  }

}
