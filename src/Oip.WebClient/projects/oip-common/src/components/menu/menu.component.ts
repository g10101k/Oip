import { Component, inject, OnInit } from '@angular/core';
import { MenuService } from "../../services/app.menu.service";
import { LayoutService } from "../../services/app.layout.service";
import { MenuItemComponent } from '../menu-item/menu-item.component';
import { NgFor, NgIf } from '@angular/common';
import { ButtonModule } from "primeng/button";
import { SecurityService } from "../../services/security.service";
import { ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from "primeng/dialog";
import { MenuItem } from "primeng/api";

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  standalone: true,
  imports: [NgFor, NgIf, MenuItemComponent, ButtonModule, ContextMenuModule, DialogModule]
})
export class MenuComponent implements OnInit {
  readonly menuService = inject(MenuService);
  readonly layoutService = inject(LayoutService);
  readonly securityService = inject(SecurityService);
  model: any[] = [];
  emptySpaceMenu: MenuItem[] | undefined;

  ngOnInit() {
    this.menuService.getMenu().then(
      menu => {
        this.model = menu;
      }
    );
    this.emptySpaceMenu = [
      { label: 'New', icon: 'pi pi-plus', click: () => {} },
      { label: 'Rename', icon: 'pi pi-file-edit'}
    ];
  }

  onSettingButtonClick($event: MouseEvent) {
    this.menuService.getAdminMenu().then(
      menu => {
        this.model = menu;
      }
    )
  }
}
