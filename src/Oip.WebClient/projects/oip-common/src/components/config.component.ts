import { Component } from '@angular/core';
import { ProfileComponent } from "./profile.component";


@Component({
  selector: 'app-config',
  template: `<div class="grid">
    <div class="col-12 xl:col-6">
      <div class="card mb-0">
        <h5>Profile</h5>
        <div class="flex align-items-center">
          <user-profile></user-profile>
        </div>
      </div>
    </div>
  </div>
  `,
  imports: [ProfileComponent]
})
export class ConfigComponent {

}
