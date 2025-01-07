import { inject, OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { LayoutService, MenuService } from "oip/common";

@Component({
  selector: 'app-menu',
  templateUrl: './app.menu.component.html'
})
export class AppMenuComponent implements OnInit {
  protected readonly menuService = inject(MenuService);
  model: any[] = [];

  constructor(public layoutService: LayoutService) {
  }

  ngOnInit() {
    this.menuService.getMenu().then(
      menu => {
        this.model = menu;
      }
    )
  }
}
