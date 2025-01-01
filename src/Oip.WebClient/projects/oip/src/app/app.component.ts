import { Component, inject, OnInit } from '@angular/core';
import { SecurityService } from "common";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  private readonly oipSecurityService = inject(SecurityService);

  constructor() {
  }

  ngOnInit() {
    this.oipSecurityService.auth();
  }
}
