import { Component, inject, OnInit } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { BaseDataService } from '../services/base-data.service';
import { Tag } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { MsgService } from '../services/msg.service';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { L10nService } from '../services/l10n.service';
import { Module } from '../api/Module';
import { ModuleDto } from '../api/data-contracts';
import { AppTitleService } from '../services/app-title.service';
import { TranslatePipe } from '@ngx-translate/core';

interface L10n {
  confirm: {
    header: string;
    message: string;
    cancel: string;
    delete: string;
  };
  title: string;
  messages: {
    deleteSuccess: string;
  };
  table: {
    deleteTooltip: string;
    currentlyLoaded: string;
    yes: string;
    no: string;
    name: string;
    moduleId: string;
  };
  refreshTooltip: string;
}

@Component({
  imports: [FormsModule, TableModule, Tag, ButtonModule, ToolbarModule, Tooltip, ConfirmDialog, TranslatePipe],
  providers: [ConfirmationService, Module],
  selector: 'app-modules',
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card w-full">
        <div class="font-semibold text-xl mb-4">
          {{ l10n.title }}
        </div>
        <div class="mb-4">
          <p-toolbar>
            <p-button
              icon="pi pi-refresh"
              rounded="true"
              severity="secondary"
              text="true"
              tooltipPosition="bottom"
              [pTooltip]="'app-modules.refreshTooltip' | translate"
              (onClick)="refreshAction()"></p-button>
          </p-toolbar>
        </div>
        <p-table class="mt-4" [paginator]="true" [rows]="100" [value]="modules">
          <ng-template pTemplate="header">
            <tr>
              <th>{{ 'app-modules.table.moduleId' | translate }}</th>
              <th>{{ 'app-modules.table.name' | translate }}</th>
              <th>{{ 'app-modules.table.currentlyLoaded' | translate }}</th>
              <th style="width: 4rem"></th>
            </tr>
          </ng-template>
          <ng-template let-module pTemplate="body">
            <tr>
              <td>{{ module.moduleId }}</td>
              <td>{{ module.name }}</td>
              <td>
                <p-tag
                  [severity]="module.currentlyLoaded ? 'success' : 'danger'"
                  [value]="
                    (module.currentlyLoaded ? 'app-modules.table.yes' : 'app-modules.table.no') | translate
                  "></p-tag>
              </td>
              <td>
                <p-button
                  icon="pi pi-trash"
                  rounded="true"
                  severity="danger"
                  text="true"
                  tooltipPosition="bottom"
                  [pTooltip]="'app-modules.table.deleteTooltip' | translate"
                  (onClick)="deleteModule(module)"></p-button>
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
  protected modules: ModuleDto[] = [];
  protected msgService = inject(MsgService);
  protected confirmationService = inject(ConfirmationService);
  protected l10nService = inject(L10nService);
  protected l10n: L10n = {} as L10n;
  protected titleService = inject(AppTitleService);
  private moduleService = inject(Module);

  async ngOnInit() {
    this.l10nService.get('app-modules').subscribe((l10n) => {
      this.l10n = l10n;
    });
    this.titleService.setTitle(this.l10n.title);
    await this.refreshAction();
  }

  async refreshAction() {
    this.modules = await this.moduleService.moduleGetModulesWithLoadStatus();
  }

  deleteModule(module: ModuleDto) {
    this.confirmationService.confirm({
      header: this.l10n.confirm.header,
      message: this.l10n.confirm.message,
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.l10n.confirm.cancel,
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.l10n.confirm.delete,
        severity: 'danger'
      },
      accept: async () => {
        this.dataService
          .sendRequest(`api/module/delete`, 'DELETE', {
            moduleId: module.moduleId
          })
          .then(() => this.refreshAction())
          .catch((error) => console.error(error));

        this.msgService.success(this.l10n.messages.deleteSuccess);
      }
    });
  }
}
