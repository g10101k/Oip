import { Component } from '@angular/core';
import { ProfileComponent } from "./profile.component";
import { Fluid } from "primeng/fluid";
import { Tooltip } from "primeng/tooltip";
import { FormsModule } from "@angular/forms";

@Component({
  selector: 'app-config',
  template: `
    <p-fluid>
      <div class="flex flex-col md:flex-row gap-4">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">Profile</div>
            <label> Photo <span pTooltip="Use photo 256x256 pixel"
                                tooltipPosition="right"
                                class="pi pi-question-circle"></span></label>
            <div class="flex justify-content-end flex-wrap">
              <user-profile></user-profile>
            </div>
          </div>
        </div>
      </div>
    </p-fluid>
  `,
  imports: [ProfileComponent, Fluid, Tooltip, FormsModule]
})
export class ConfigComponent {

}
