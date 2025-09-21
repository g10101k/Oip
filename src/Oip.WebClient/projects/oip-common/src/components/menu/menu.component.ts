import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MenuService } from "../../services/app.menu.service";
import { NgFor, NgIf } from '@angular/common';
import { ButtonModule } from "primeng/button";
import { SecurityService } from "../../services/security.service";
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from "primeng/dialog";
import { MenuItemCommandEvent, PrimeIcons } from "primeng/api";
import { InputTextModule } from "primeng/inputtext";
import { InputSwitchModule } from 'primeng/inputswitch';
import { FormsModule } from "@angular/forms";
import { MenuItemComponent } from './menu-item.component';
import { MenuItemCreateDialogComponent } from "./menu-item-create-dialog.component";
import { TranslateService } from "@ngx-translate/core";
import { MenuItemEditDialogComponent } from "./menu-item-edit-dialog.component";
import { Menu } from "../../api/Menu";


@Component({
  imports: [NgFor, NgIf, MenuItemComponent, ButtonModule, ContextMenuModule, DialogModule, InputTextModule, MenuItemCreateDialogComponent, InputSwitchModule, FormsModule, MenuItemEditDialogComponent],
  providers: [Menu],
  selector: 'app-menu',
  standalone: true,
  template: `
    <div #emtpty class="layout-sidebar" (contextmenu)="onContextMenu($event)">
      <ul class="layout-menu">
        <ng-container *ngFor="let item of menuService.menu; let i = index;">
          <li app-menuitem
              *ngIf="!item.separator"
              [item]="item"
              [index]="i"
              [root]="true"
              [menuItemCreateDialogComponent]="menuItemCreateDialogComponent"
              [menuItemEditDialogComponent]="menuItemEditDialogComponent"
              [contextMenu]="contextMenu"></li>
          <li *ngIf="item.separator" class="menu-separator"></li>
        </ng-container>
      </ul>
    </div>
    <p-contextMenu [target]="emtpty"/>
    <menu-item-create-dialog *ngIf="securityService.isAdmin"/>
    <menu-item-edit-dialog *ngIf="securityService.isAdmin"/>`
})
export class MenuComponent implements OnInit {
  readonly menuService = inject(MenuService);
  readonly securityService = inject(SecurityService);
  readonly translateService = inject(TranslateService);

  @ViewChild(MenuItemCreateDialogComponent) menuItemCreateDialogComponent: MenuItemCreateDialogComponent;
  @ViewChild(MenuItemEditDialogComponent) menuItemEditDialogComponent: MenuItemEditDialogComponent;
  @ViewChild(ContextMenu) contextMenu: ContextMenu;

  ngOnInit() {
    this.menuService.loadMenu().then();
  }

  private newClick(e: MenuItemCommandEvent) {
    this.menuItemCreateDialogComponent.showDialog();
  }

  onContextMenu($event: MouseEvent) {
    this.menuService.contextMenuItem = null;
    this.contextMenu.model = [
      {
        label: this.translateService.instant('menuComponent.new'),
        icon: PrimeIcons.PLUS,
        command: (event) => this.newClick(event)
      },
    ];
  }
}
