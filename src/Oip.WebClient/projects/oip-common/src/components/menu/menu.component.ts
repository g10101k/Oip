import { Component, inject, OnInit } from '@angular/core';
import { MenuService } from "../../services/app.menu.service";
import { LayoutService } from "../../services/app.layout.service";

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html'
})
export class MenuComponent implements OnInit {
  protected readonly menuService = inject(MenuService);
  public readonly layoutService = inject(LayoutService);
  model: any[] = [];

  ngOnInit() {
    this.menuService.getMenu().then(
      menu => {
        this.model = menu;
      }
    )
  }
}
