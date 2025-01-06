import { Component, inject, OnInit } from '@angular/core';
import { SecurityService } from "oip/common";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  private readonly securityService = inject(SecurityService);

  constructor() {
  }

  ngOnInit() {
    this.securityService.auth();
  }
}
