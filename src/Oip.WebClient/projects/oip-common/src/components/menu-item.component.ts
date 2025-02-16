import {
  ChangeDetectorRef,
  Component,
  HostBinding,
  inject,
  Input,
  OnDestroy,
  OnInit
} from '@angular/core';
import { NavigationEnd, Router, RouterLinkActive, RouterLink } from '@angular/router';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { LayoutService } from "./../services/app.layout.service";
import { MenuService } from "./../services/app.menu.service";
import { RippleModule } from 'primeng/ripple';
import { NgIf, NgClass, NgFor } from '@angular/common';
import { MenuItem, MenuItemCommandEvent, PrimeIcons } from "primeng/api";
import { CreateMenuItemDialogComponent } from "./create-menu-item-dialog.component";
import { ContextMenu, ContextMenuModule } from "primeng/contextmenu";
import { MsgService } from "./../services/msg.service";

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: '[app-menuitem]',
  template: `
    <ng-container>
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
              [createMenuItemDialogComponent]="createMenuItemDialogComponent"
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
  standalone: true,
  imports: [NgIf, RippleModule, NgClass, RouterLinkActive, RouterLink, NgFor, ContextMenuModule]
})
export class MenuItemComponent implements OnInit, OnDestroy {
  public layoutService: LayoutService = inject(LayoutService);
  @Input() item: any;
  @Input() index!: number;
  @Input() @HostBinding('class.layout-root-menuitem') root!: boolean;
  @Input() parentKey!: string;
  @Input() createMenuItemDialogComponent: CreateMenuItemDialogComponent;
  @Input() contextMenu: ContextMenu;

  active = false;

  menuSourceSubscription: Subscription;

  menuResetSubscription: Subscription;

  key: string = "";
  itemMenu: MenuItem[] = [
    { label: 'New', icon: PrimeIcons.PLUS, command: (event) => this.newClick(event) },
    { label: 'Edit', icon: PrimeIcons.FILE_EDIT },
    { separator: true },
    { label: 'Delete', icon: PrimeIcons.TRASH, command: (event) => this.deleteItem(event) },
  ];
  msgService = inject(MsgService);

  constructor(private readonly cd: ChangeDetectorRef, public router: Router, private readonly menuService: MenuService) {
    this.menuSourceSubscription = this.menuService.menuSource$.subscribe(value => {
      Promise.resolve(null).then(() => {
        if (value.routeEvent) {
          this.active = (value.key === this.key || value.key.startsWith(this.key + '-')) ? true : false;
        } else if (value.key !== this.key && !value.key.startsWith(this.key + '-')) {
          this.active = false;
        }
      });
    });

    this.menuResetSubscription = this.menuService.resetSource$.subscribe(() => {
      this.active = false;
    });

    this.router.events.pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(params => {
        if (this.item.routerLink) {
          this.updateActiveStateFromRoute();
        }
      });
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
      this.menuService.onMenuStateChange({ key: this.key, routeEvent: true });
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

    this.menuService.onMenuStateChange({ key: this.key });
  }

  get submenuAnimation() {
    return this.root ? 'expanded' : (this.active ? 'expanded' : 'collapsed');
  }

  @HostBinding('class.active-menuitem')
  get activeClass() {
    return this.active && !this.root;
  }

  ngOnDestroy() {
    if (this.menuSourceSubscription) {
      this.menuSourceSubscription.unsubscribe();
    }

    if (this.menuResetSubscription) {
      this.menuResetSubscription.unsubscribe();
    }
  }

  private newClick(e: MenuItemCommandEvent) {
    this.createMenuItemDialogComponent.showDialog();
  }

  onContextMenu($event: MouseEvent, item: any) {
    this.menuService.contextMenuItem = item;
    this.contextMenu.model = this.itemMenu;
    this.contextMenu.show($event);
  }

  private deleteItem(event: MenuItemCommandEvent) {
    this.menuService.deleteItem(this.menuService.contextMenuItem?.moduleInstanceId).then(() => {
      this.msgService.success('Saved security');
    }, error => this.msgService.error(error));
  }
}
