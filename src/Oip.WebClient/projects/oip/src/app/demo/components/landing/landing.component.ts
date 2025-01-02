import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { StyleClass } from 'primeng/styleclass';
import { ButtonDirective } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { AppConfigService } from "../../../layout/service/appconfigservice";

@Component({
    selector: 'app-landing',
    templateUrl: './landing.component.html',
    imports: [StyleClass, ButtonDirective, Divider]
})
export class LandingComponent {

  constructor(public layoutService: AppConfigService, public router: Router) {
  }

}
