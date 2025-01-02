import { Component, Input } from '@angular/core';
import { MenuService } from '../app.menu.service';
import { ButtonDirective } from 'primeng/button';
import { NgFor, NgClass, NgIf } from '@angular/common';
import { RadioButton } from 'primeng/radiobutton';
import { FormsModule } from '@angular/forms';
import { InputSwitch } from 'primeng/inputswitch';
import { AppConfiguratorComponent } from "../configurator/app.configurator.component";

@Component({
  selector: 'app-config',
  templateUrl: './app.config.component.html',
  imports: [ButtonDirective, NgFor, NgClass, NgIf, RadioButton, FormsModule, InputSwitch, AppConfiguratorComponent]
})
export class AppConfigComponent {
  @Input() minimal: boolean = false;

  scales: number[] = [12, 13, 14, 15, 16];

  constructor(
    public menuService: MenuService
  ) {
  }

}
