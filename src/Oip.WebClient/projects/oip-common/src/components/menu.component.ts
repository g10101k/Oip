import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MenuService } from "../services/app.menu.service";
import { LayoutService } from "../services/app.layout.service";
import { NgFor, NgIf } from '@angular/common';
import { ButtonModule } from "primeng/button";
import { SecurityService } from "./../services/security.service";
import { ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from "primeng/dialog";
import { MenuItem, MenuItemCommandEvent, PrimeIcons } from "primeng/api";
import { InputTextModule } from "primeng/inputtext";
import { MsgService } from './../services/msg.service';
import { InputSwitchModule } from 'primeng/inputswitch';
import { FormsModule } from "@angular/forms";
import { MenuItemComponent } from './menu-item.component';
import { CreateMenuItemDialogComponent } from "./create-menu-item-dialog.component";


@Component({
  selector: 'app-menu',
  template: `<div #emtpty class="layout-sidebar">
    <ul class="layout-menu">
      <ng-container *ngFor="let item of model; let i = index;">
        <li app-menuitem *ngIf="!item.separator" [item]="item" [index]="i" [root]="true" ></li>
        <li *ngIf="item.separator" class="menu-separator"></li>
      </ng-container>
      <div *ngIf="securityService.isAdmin" class="flex items-center absolute right-0 bottom-0 m-4">
        <label for="adminMode" class="mr-2">All</label>
        <p-inputSwitch id="adminMode" [(ngModel)]="adminMode" (onChange)="onSettingButtonClick()"></p-inputSwitch>
      </div>
      <p-contextMenu [target]="emtpty" [model]="emptySpaceMenu"/>
    </ul>
  </div>
  <create-menu-item-dialog/>`,
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
