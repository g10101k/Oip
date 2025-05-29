import { Component, inject, OnInit } from '@angular/core';
import { Tooltip } from "primeng/tooltip";
import { FormsModule } from "@angular/forms";
import { TableModule } from "primeng/table";
import { TranslatePipe } from "@ngx-translate/core";
import { BaseDataService } from "./../services/base-data.service";
import { Tag } from "primeng/tag";

export interface ExistModuleDto {
  moduleId: number;
  name: string;
  currentlyLoaded: boolean;
}
@Component({
  selector: 'app-modules',
  template: `
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card flex flex-col gap-4">
        <div class="font-semibold text-xl">Modules</div>
        <p-table [value]="modules" [paginator]="true" [rows]="10" [responsiveLayout]="'scroll'">
          <ng-template pTemplate="header">
            <tr>
              <th>Module ID</th>
              <th>Name</th>
              <th>Currently Loaded</th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-module>
            <tr>
              <td>{{ module.moduleId }}</td>
              <td>{{ module.name }}</td>
              <td>
                <p-tag
                  [value]="module.currentlyLoaded ? 'Yes' : 'No'"
                  [severity]="module.currentlyLoaded ? 'success' : 'danger'"
                ></p-tag>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    </div>
  `,
  imports: [ FormsModule, TableModule, TranslatePipe, Tag]
})
export class AppModulesComponent implements OnInit {
  protected dataService = inject(BaseDataService);
  protected modules: ExistModuleDto[] = [];

  ngOnInit(): void {
    this.dataService.sendRequest<ExistModuleDto[]>(`api/module/get-modules-with-load-status`).then((data) => {
      this.modules = data;
    }).catch((error) => {
      console.error(error);
    })
  }
}
