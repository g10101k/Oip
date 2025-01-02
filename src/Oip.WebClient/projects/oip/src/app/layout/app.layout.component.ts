import { Component, OnDestroy, OnInit, Renderer2, ViewChild } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AppSidebarComponent } from "./app.sidebar.component";
import { AppTopBarComponent } from './app.topbar.component';
import { NgClass } from '@angular/common';
import { AppFooterComponent } from './app.footer.component';
import { AppConfigService } from "./service/appconfigservice";

@Component({
  selector: 'app-layout',
  templateUrl: './app.layout.component.html',
  imports: [NgClass, AppTopBarComponent, AppSidebarComponent, RouterOutlet, AppFooterComponent]
})
export class AppLayoutComponent implements OnDestroy, OnInit {

  overlayMenuOpenSubscription: Subscription;

  menuOutsideClickListener: any;

  profileMenuOutsideClickListener: any;

  @ViewChild(AppSidebarComponent) appSidebar!: AppSidebarComponent;

  @ViewChild(AppTopBarComponent) appTopbar!: AppTopBarComponent;

  constructor(public layoutService: AppConfigService, public renderer: Renderer2, public router: Router) {

  }

  ngOnInit(): void {
    this.overlayMenuOpenSubscription = this.layoutService.overlayOpen$.subscribe(() => {
      if (!this.menuOutsideClickListener) {
        this.menuOutsideClickListener = this.renderer.listen('document', 'click', event => {
          const isOutsideClicked = !(this.appSidebar.el.nativeElement.isSameNode(event.target) || this.appSidebar.el.nativeElement.contains(event.target)
            || this.appTopbar.menuButton.nativeElement.isSameNode(event.target) || this.appTopbar.menuButton.nativeElement.contains(event.target));

          if (isOutsideClicked) {
            this.hideMenu();
          }
        });
      }

      if (!this.profileMenuOutsideClickListener) {
        this.profileMenuOutsideClickListener = this.renderer.listen('document', 'click', event => {
          const isOutsideClicked = !(this.appTopbar.menu.nativeElement.isSameNode(event.target) || this.appTopbar.menu.nativeElement.contains(event.target)
            || this.appTopbar.topbarMenuButton.nativeElement.isSameNode(event.target) || this.appTopbar.topbarMenuButton.nativeElement.contains(event.target));

          if (isOutsideClicked) {
            this.hideProfileMenu();
          }
        });
      }

      if (this.layoutService.appState().staticMenuMobileActive) {
        this.blockBodyScroll();
      }
    });

    this.router.events.pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.hideMenu();
        this.hideProfileMenu();
      });
  }

  hideMenu() {
    this.layoutService.appState().overlayMenuActive = false;
    this.layoutService.appState().staticMenuMobileActive = false;
    this.layoutService.appState().menuHoverActive = false;
    if (this.menuOutsideClickListener) {
      this.menuOutsideClickListener();
      this.menuOutsideClickListener = null;
    }
    this.unblockBodyScroll();
  }

  hideProfileMenu() {
    this.layoutService.appState().profileSidebarVisible = false;
    if (this.profileMenuOutsideClickListener) {
      this.profileMenuOutsideClickListener();
      this.profileMenuOutsideClickListener = null;
    }
  }

  blockBodyScroll(): void {
    if (document.body.classList) {
      document.body.classList.add('blocked-scroll');
    } else {
      document.body.className += ' blocked-scroll';
    }
  }

  unblockBodyScroll(): void {
    if (document.body.classList) {
      document.body.classList.remove('blocked-scroll');
    } else {
      document.body.className = document.body.className.replace(new RegExp('(^|\\b)' +
        'blocked-scroll'.split(' ').join('|') + '(\\b|$)', 'gi'), ' ');
    }
  }

  get containerClass() {
    return {
      'layout-theme-light': this.layoutService.appState().colorScheme === 'light',
      'layout-theme-dark': this.layoutService.appState().colorScheme === 'dark',
      'layout-overlay': this.layoutService.appState().menuMode === 'overlay',
      'layout-static': this.layoutService.appState().menuMode === 'static',
      'layout-static-inactive': this.layoutService.appState().staticMenuDesktopInactive && this.layoutService.appState().menuMode === 'static',
      'layout-overlay-active': this.layoutService.appState().overlayMenuActive,
      'layout-mobile-active': this.layoutService.appState().staticMenuMobileActive,
      'p-input-filled': this.layoutService.appState().inputStyle === 'filled',
      'p-ripple-disabled': !this.layoutService.appState().ripple
    }
  }

  ngOnDestroy() {
    if (this.overlayMenuOpenSubscription) {
      this.overlayMenuOpenSubscription.unsubscribe();
    }

    if (this.menuOutsideClickListener) {
      this.menuOutsideClickListener();
    }
  }
}
