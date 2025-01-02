import { Component } from '@angular/core';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { FormsModule } from '@angular/forms';
import { Checkbox } from 'primeng/checkbox';
import { ButtonDirective } from 'primeng/button';
import { RouterLink } from '@angular/router';
import { AppConfigService } from "../../../../layout/service/appconfigservice";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styles: [`
    :host ::ng-deep .pi-eye,
    :host ::ng-deep .pi-eye-slash {
      transform: scale(1.6);
      margin-right: 1rem;
      color: var(--primary-color) !important;
    }
  `],
  imports: [InputText, Password, FormsModule, Checkbox, ButtonDirective, RouterLink]
})
export class LoginComponent {

  valCheck: string[] = ['remember'];

  password!: string;

  constructor(public layoutService: AppConfigService) {
  }
}
