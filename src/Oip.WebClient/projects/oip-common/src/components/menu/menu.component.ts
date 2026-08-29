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
import { MenuApi } from '../../api/menu.api';

import { provideTranslations } from '../../helpers/l10n.helper';
import en from './l10n/menu.en.json';
import ru from './l10n/menu.ru.json';

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
  providers: [MenuApi],
  selector: 'app-menu',
  standalone: true,
  template: ` <div #empty class="layout-sidebar" (contextmenu)="onContextMenu($event)">
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
    @if (securityService.isAdmin()) {
      <p-contextMenu [target]="empty" />
      <menu-item-create-dialog />
      <menu-item-edit-dialog />
    }`
})
export class MenuComponent implements OnInit {
  // Registers the whole menu/l10n/menu.*.json bundle, which also covers
  // menuItemComponent, menuItemEditDialogComponent and menuItemCreateDialogComponent.
  private readonly translations = provideTranslations({ en, ru });

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

  protected onContextMenu($event: MouseEvent) {
    if (!this.securityService.isAdmin()) {
      return;
    }

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
