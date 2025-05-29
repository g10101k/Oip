import { Component, inject, OnInit } from '@angular/core';
import { Tooltip } from "primeng/tooltip";
import { FormsModule } from "@angular/forms";
import { TableModule } from "primeng/table";
import { TranslatePipe } from "@ngx-translate/core";
import { BaseDataService } from "./../services/base-data.service";
import { Tag } from "primeng/tag";
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';

export interface ExistModuleDto {
  moduleId: number;
  name: string;
  currentlyLoaded: boolean;
}

@Component({
  imports: [FormsModule, TableModule, TranslatePipe, Tag, ButtonModule, ToolbarModule, Tooltip],
  selector: 'app-modules',
  template: `
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card w-full">
        <div class="font-semibold text-xl">Modules</div>
        <p-toolbar class="mb-4">
          <p-button icon="pi pi-refresh"
                    severity="secondary"
                    (onClick)="loadModules()"
                    pTooltip="Refresh"
                    tooltipPosition="bottom"
                    rounded="true"
                    text="true"></p-button>
        </p-toolbar>
        <p-table class="mt-4" [value]="modules" [paginator]="true" [rows]="100">
          <ng-template pTemplate="header">
            <tr>
              <th>Module ID</th>
              <th>Name</th>
              <th>Currently Loaded</th>
              <th style="width: 4rem"></th> <!-- Column for delete button -->
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
              <td>
                <p-button
                  icon="pi pi-trash"
                  (onClick)="deleteModule(module)"
                  pTooltip="Delete"
                  tooltipPosition="bottom"
                  severity="danger"
                  rounded="true"
                  text="true"
                ></p-button>
              </td>
            </tr>
          </ng-template>
        </p-table>

      </div>
    </div>
  `
})
export class AppModulesComponent implements OnInit {
  protected dataService = inject(BaseDataService);
  protected modules: ExistModuleDto[] = [];

  ngOnInit(): void {
    this.loadModules();
  }

  loadModules(): void {
    this.dataService
      .sendRequest<ExistModuleDto[]>(`api/module/get-modules-with-load-status`)
      .then(data => this.modules = data)
      .catch(error => console.error(error));
  }

  deleteModule(module: ExistModuleDto) {
    this.dataService
      .sendRequest(`api/module/delete`, "DELETE", { moduleId: module.moduleId})
      .then()
      .catch(error => console.error(error));
  }
}
