import { Component, inject, OnInit } from '@angular/core';
import { LoginResponse } from 'angular-auth-oidc-client';
import { OipSecurityService } from "common";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  private readonly oipSecurityService = inject(OipSecurityService);

  constructor() {
  }

  ngOnInit() {
    this.oipSecurityService.checkAuth().subscribe((loginResponse: LoginResponse) => {
      this.oipSecurityService.getPayloadFromAccessToken().subscribe(x => {
          console.log(x);
        }
      );
    });
  }
}
