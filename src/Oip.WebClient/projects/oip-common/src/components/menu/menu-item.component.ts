import { ChangeDetectorRef, Component, HostBinding, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterLinkActive, RouterLink } from '@angular/router';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { LayoutService } from "../../services/app.layout.service";
import { MenuService } from "../../services/app.menu.service";
import { RippleModule } from 'primeng/ripple';
import { NgIf, NgClass, NgFor } from '@angular/common';
import { ConfirmationService, MenuItem, MenuItemCommandEvent, PrimeIcons } from "primeng/api";
import { MenuItemCreateDialogComponent } from "./menu-item-create-dialog.component";
import { ContextMenu, ContextMenuModule } from "primeng/contextmenu";
import { MsgService } from "../../services/msg.service";
import { MenuItemEditDialogComponent } from "./menu-item-edit-dialog.component";
import { ContextMenuItemDto } from "../../dtos/context-menu-item.dto";
import { TranslateService } from "@ngx-translate/core";
import { ConfirmDialog } from "primeng/confirmdialog";

interface MenuItemComponentTranslation {
  delete: string;
  edit: string;
  new: string;
  deleteItemConfirmHeader: string;
  deleteItemConfirmMessage: string;
  deleteItemSuccessMessage: string;
  deleteItemConfirmRejectButtonPropsLabel: string;
  deleteItemConfirmAcceptButtonPropsLabel: string;
}

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: '[app-menuitem]',
  template: `
    <ng-container>
      <p-confirm-dialog/>
      <div *ngIf="root && item.visible !== false" class="layout-menuitem-root-text"
           (contextmenu)="onContextMenu($event, item)">{{ item.label }}
      </div>
      <a *ngIf="(!item.routerLink || item.items) && item.visible !== false" [attr.href]="item.url"
         (click)="itemClick($event)"
         [ngClass]="item.class"
         [attr.target]="item.target"
         tabindex="0" pRipple
      >
        <i [ngClass]="item.icon" class="layout-menuitem-icon"></i>
        <span class="layout-menuitem-text">{{ item.label }}</span>
        <i class="pi pi-fw pi-angle-down layout-submenu-toggler" *ngIf="item.items"></i>
      </a>
      <a *ngIf="(item.routerLink && !item.items) && item.visible !== false" (click)="itemClick($event) "
         [ngClass]="item.class"
         [routerLink]="item.routerLink" routerLinkActive="active-route"
         [routerLinkActiveOptions]="item.routerLinkActiveOptions||{ paths: 'exact', queryParams: 'ignored', matrixParams: 'ignored', fragment: 'ignored' }"
         [fragment]="item.fragment" [queryParamsHandling]="item.queryParamsHandling"
         [preserveFragment]="item.preserveFragment"
         [skipLocationChange]="item.skipLocationChange" [replaceUrl]="item.replaceUrl" [state]="item.state"
         [queryParams]="item.queryParams"
         [attr.target]="item.target" tabindex="0" pRipple
         (contextmenu)="onContextMenu($event, item)">
        <i [ngClass]="item.icon" class="layout-menuitem-icon"></i>
        <span class="layout-menuitem-text">{{ item.label }}</span>
        <i class="pi pi-fw pi-angle-down layout-submenu-toggler" *ngIf="item.items"></i>
      </a>

      <ul *ngIf="item.items && item.visible !== false" [@children]="submenuAnimation"
          (contextmenu)="onContextMenu($event, item)">
        <ng-template ngFor let-child let-i="index" [ngForOf]="item.items">
          <li app-menuitem [item]="child" [index]="i" [parentKey]="key" [class]="child.badgeClass"
              [menuItemCreateDialogComponent]="menuItemCreateDialogComponent"
              [menuItemEditDialogComponent]="menuItemEditDialogComponent"
              [contextMenu]="contextMenu"></li>
        </ng-template>
      </ul>
    </ng-container>
  `,
  animations: [
    trigger('children', [
      state('collapsed', style({
        height: '0'
      })),
      state('expanded', style({
        height: '*'
      })),
      transition('collapsed <=> expanded', animate('400ms cubic-bezier(0.86, 0, 0.07, 1)'))
    ])
  ],
  imports: [NgIf, RippleModule, NgClass, RouterLinkActive, RouterLink, NgFor, ContextMenuModule, ConfirmDialog],
  providers: [ConfirmationService]
})
export class MenuItemComponent implements OnInit, OnDestroy {
  private readonly layoutService = inject(LayoutService);
  private readonly translateService = inject(TranslateService);
  private readonly confirmationService = inject(ConfirmationService)
  private readonly msgService = inject(MsgService);

  @Input() item: ContextMenuItemDto;
  @Input() index!: number;
  @Input() @HostBinding('class.layout-root-menuitem') root!: boolean;
  @Input() parentKey!: string;
  @Input() menuItemCreateDialogComponent: MenuItemCreateDialogComponent;
  @Input() menuItemEditDialogComponent: MenuItemEditDialogComponent;
  @Input() contextMenu: ContextMenu;

  private active = false;
  private subscriptions: Subscription[] = [];
  private localization: MenuItemComponentTranslation = {} as MenuItemComponentTranslation;

  private key: string = "";

  constructor(private readonly cd: ChangeDetectorRef, public router: Router, private readonly menuService: MenuService) {
    this.subscriptions.push(this.menuService.menuSource$.subscribe(value => {
      Promise.resolve(null).then(() => {
        if (value.routeEvent) {
          this.active = (value.key === this.key || value.key.startsWith(this.key + '-'));
        } else if (value.key !== this.key && !value.key.startsWith(this.key + '-')) {
          this.active = false;
        }
      });
    }));

    this.subscriptions.push(this.menuService.resetSource$.subscribe(() => {
      this.active = false;
    }));

    this.subscriptions.push(this.router.events.pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(params => {
        if (this.item.routerLink) {
          this.updateActiveStateFromRoute();
        }
      }));

    this.subscriptions.push(this.translateService.get('menuItemComponent').subscribe((value: MenuItemComponentTranslation) => {
      this.localization = value;
    }));
  }

  ngOnInit() {
    this.key = this.parentKey ? this.parentKey + '-' + this.index : String(this.index);

    if (this.item.routerLink) {
      this.updateActiveStateFromRoute();
    }
  }

  updateActiveStateFromRoute() {
    let activeRoute = this.router.isActive(this.item.routerLink[0], {
      paths: 'exact',
      queryParams: 'ignored',
      matrixParams: 'ignored',
      fragment: 'ignored'
    });

    if (activeRoute) {
      this.menuService.onMenuStateChange({ key: this.key, item: this.item, routeEvent: true });
    }
  }

  itemClick(event: Event) {
    // avoid processing disabled items
    if (this.item.disabled) {
      event.preventDefault();
      return;
    }

    // execute command
    if (this.item.command) {
      this.item.command({ originalEvent: event, item: this.item });
    }

    // toggle active state
    if (this.item.items) {
      this.active = !this.active;
    }

    this.menuService.onMenuStateChange({ key: this.key, item: this.item });
  }

  get submenuAnimation() {
    return (this.root || this.active) ? 'expanded' : 'collapsed';
  }

  @HostBinding('class.active-menuitem')
  get activeClass() {
    return this.active && !this.root;
  }

  ngOnDestroy() {
    this.subscriptions.map(s => s.unsubscribe());
  }

  private newClick(e: MenuItemCommandEvent) {
    this.menuItemCreateDialogComponent.showDialog();
  }

  onContextMenu($event: MouseEvent, item: any) {
    this.menuService.contextMenuItem = item;
    this.contextMenu.model = [
      {
        label: this.localization.new,
        icon: PrimeIcons.PLUS,
        command: (event) => this.newClick(event)
      },
      {
        label: this.localization.edit,
        icon: PrimeIcons.FILE_EDIT,
        command: (event) => this.editClick(event)
      },
      { separator: true },
      {
        label: this.localization.delete,
        icon: PrimeIcons.TRASH,
        command: (event) => this.deleteItem(event)
      },
    ];
    this.contextMenu.show($event);
  }

  private deleteItem(event: MenuItemCommandEvent) {
    this.confirmationService.confirm({
      header: this.localization.deleteItemConfirmHeader,
      message: this.localization.deleteItemConfirmMessage,
      icon: PrimeIcons.TRASH,
      rejectButtonProps: {
        label: this.localization.deleteItemConfirmRejectButtonPropsLabel,
        severity: 'secondary',
        outlined: true,
      },
      acceptButtonProps: {
        label: this.localization.deleteItemConfirmAcceptButtonPropsLabel,
        severity: 'danger',
      },
      accept: async () => {
        await this.menuService.deleteItem(this.menuService.contextMenuItem?.moduleInstanceId);
        this.msgService.success(this.localization.deleteItemSuccessMessage);
        await this.menuService.loadMenu();
      },
    });
  }

  private editClick(event: MenuItemCommandEvent) {
    this.menuItemEditDialogComponent.showDialog();
  }
}
