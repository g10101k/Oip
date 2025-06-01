import { Component, inject, OnInit } from '@angular/core';
import { Tooltip } from "primeng/tooltip";
import { FormsModule } from "@angular/forms";
import { TableModule } from "primeng/table";
import { TranslatePipe, TranslateService } from "@ngx-translate/core";
import { BaseDataService } from "./../services/base-data.service";
import { Tag } from "primeng/tag";
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { MsgService } from "../services/msg.service";
import { ConfirmationService } from "primeng/api";
import { ConfirmDialog } from "primeng/confirmdialog";

export interface ExistModuleDto {
  moduleId: number;
  name: string;
  currentlyLoaded: boolean;
}

@Component({
  imports: [FormsModule, TableModule, TranslatePipe, Tag, ButtonModule, ToolbarModule, Tooltip, ConfirmDialog],
  providers: [ConfirmationService],
  selector: 'app-modules',
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card w-full">
        <div class="font-semibold text-xl mb-4">
          {{ 'appModulesComponent.title' | translate }}
        </div>
        <div class="mb-4">
          <p-toolbar>
            <p-button
              icon="pi pi-refresh"
              severity="secondary"
              (onClick)="refreshAction()"
              [pTooltip]="'appModulesComponent.refreshTooltip' | translate"
              tooltipPosition="bottom"
              rounded="true"
              text="true"
            ></p-button>
          </p-toolbar>
        </div>
        <p-table class="mt-4" [value]="modules" [paginator]="true" [rows]="100">
          <ng-template pTemplate="header">
            <tr>
              <th>{{ 'appModulesComponent.table.moduleId' | translate }}</th>
              <th>{{ 'appModulesComponent.table.name' | translate }}</th>
              <th>{{ 'appModulesComponent.table.currentlyLoaded' | translate }}</th>
              <th style="width: 4rem"></th>
            </tr>
          </ng-template>
          <ng-template pTemplate="body" let-module>
            <tr>
              <td>{{ module.moduleId }}</td>
              <td>{{ module.name }}</td>
              <td>
                <p-tag
                  [value]="module.currentlyLoaded
                ? ('appModulesComponent.table.yes' | translate)
                : ('appModulesComponent.table.no' | translate)"
                  [severity]="module.currentlyLoaded ? 'success' : 'danger'"
                ></p-tag>
              </td>
              <td>
                <p-button
                  icon="pi pi-trash"
                  (onClick)="deleteModule(module)"
                  [pTooltip]="'appModulesComponent.table.deleteTooltip' | translate"
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
  private msgService = inject(MsgService);
  private confirmationService = inject(ConfirmationService);
  private translate = inject(TranslateService);

  ngOnInit(): void {
    this.refreshAction();
  }

  refreshAction(): void {
    this.dataService
      .sendRequest<ExistModuleDto[]>(`api/module/get-modules-with-load-status`)
      .then(data => this.modules = data)
      .catch(error => console.error(error));
  }

  deleteModule(module: ExistModuleDto) {
    console.log(module);
    this.confirmationService.confirm({
      header: this.translate.instant('appModulesComponent.confirm.header'),
      message: this.translate.instant('appModulesComponent.confirm.message'),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.translate.instant('appModulesComponent.confirm.cancel'),
        severity: 'secondary',
        outlined: true,
      },
      acceptButtonProps: {
        label: this.translate.instant('appModulesComponent.confirm.delete'),
        severity: 'danger',
      },
      accept: async () => {
        this.dataService
          .sendRequest(`api/module/delete`, 'DELETE', { moduleId: module.moduleId })
          .then(() => this.refreshAction())
          .catch(error => console.error(error));

        this.msgService.success(
          this.translate.instant('appModulesComponent.messages.deleteSuccess')
        );
      },
    });
  }
}
