import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MenuService } from '../../services/app.menu.service';
import { ButtonModule } from 'primeng/button';
import { SecurityService } from '../../services/security.service';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from 'primeng/dialog';
import { MenuItemCommandEvent, PrimeIcons } from 'primeng/api';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { MenuItemComponent } from './menu-item.component';
import { MenuItemCreateDialogComponent } from './menu-item-create-dialog.component';
import { TranslateService } from '@ngx-translate/core';
import { MenuItemEditDialogComponent } from './menu-item-edit-dialog.component';
import { Menu } from '../../api/Menu';

@Component({
  imports: [
    MenuItemComponent,
    ButtonModule,
    ContextMenuModule,
    DialogModule,
    InputTextModule,
    MenuItemCreateDialogComponent,
    FormsModule,
    MenuItemEditDialogComponent
  ],
  providers: [Menu],
  selector: 'app-menu',
  standalone: true,
  template: `
    <div #empty class="layout-sidebar" (contextmenu)="onContextMenu($event)">
      <ul class="layout-menu">
        @for (item of menuService.menu; track item; let i = $index) {
          <ng-container>
            @if (item.separator) {
              <li class="menu-separator"></li>
            } @else {
              <li
                app-menuitem
                [contextMenu]="contextMenu"
                [index]="i"
                [item]="item"
                [menuItemCreateDialogComponent]="menuItemCreateDialogComponent"
                [menuItemEditDialogComponent]="menuItemEditDialogComponent"
                [root]="true"></li>
            }
          </ng-container>
        }
      </ul>
    </div>
    <p-contextMenu [target]="empty"/>
    @if (securityService.isAdmin) {
      <menu-item-create-dialog/>
      <menu-item-edit-dialog/>
    }`
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
      }
    ];
  }
}
