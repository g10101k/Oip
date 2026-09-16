import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, SharedModule } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Tooltip } from 'primeng/tooltip';
import { BaseModuleComponent } from '../base-module/base-module.component';
import { NoSettingsDto } from '../../dtos/no-settings.dto';
import { SecurityComponent } from '../security/security.component';
import { TranslatePipe } from '@ngx-translate/core';
import { provideTranslations } from '../../helpers/l10n.helper';
import en from './l10n/db-migration.en.json';
import ru from './l10n/db-migration.ru.json';

export interface MigrationDto {
  name: string;
  applied: boolean;
  pending: boolean;
  exist: boolean;
}

export interface ApplyMigrationRequest {
  name: string;
}

@Component({
  imports: [
    TableModule,
    SharedModule,
    TagModule,
    InputTextModule,
    ButtonModule,
    ToolbarModule,
    FormsModule,
    ConfirmDialog,
    SecurityComponent,
    Tooltip,
    TranslatePipe
  ],
  selector: 'db-migration',
  template: `
    @if (isContent) {
      <p-confirmDialog />
      <div class="flex flex-col md:flex-row gap-4">
        <div class="card w-full">
          <div class="mb-4">
            <p-toolbar>
              <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
                <div class="font-semibold text-lg flex items-center gap-2 mx-2">
                  <i class="pi pi-database"></i>
                  {{ 'db-migration.migrationManager' | translate }}
                </div>

                <div class="flex-1"></div>
                <div class="flex items-center gap-1 w-full md:w-auto">
                  <p-button
                    icon="pi pi-refresh"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="bottom"
                    [loading]="loading"
                    [pTooltip]="'db-migration.actions.refresh' | translate"
                    (onClick)="refreshAction()"></p-button>
                  <input
                    class="w-full md:w-96"
                    pInputText
                    type="text"
                    [placeholder]="'db-migration.filterPlaceholder' | translate"
                    [(ngModel)]="globalFilter"
                    (ngModelChange)="dt.filterGlobal($event, 'contains')" />
                  <p-button
                    icon="pi pi-filter-slash"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="bottom"
                    [disabled]="!hasActiveFilters"
                    [pTooltip]="'db-migration.actions.cleanFilter' | translate"
                    (onClick)="clearFilter()"></p-button>
                </div>
              </div>
            </p-toolbar>
          </div>
          <p-table
            #dt
            class="mt-4"
            dataKey="name"
            sortField="name"
            [sortOrder]="1"
            [paginator]="true"
            [rows]="50"
            [globalFilterFields]="['name']"
            [value]="data"
            [loading]="loading">
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="name" scope="col">
                  {{ 'db-migration.columns.name' | translate }}
                  <p-sortIcon field="name"></p-sortIcon>
                  <p-columnFilter display="menu" field="name" type="text" />
                </th>
                <th pSortableColumn="applied" scope="col">
                  {{ 'db-migration.columns.applied' | translate }}
                  <p-sortIcon field="applied"></p-sortIcon>
                </th>
                <th pSortableColumn="exist" scope="col">
                  {{ 'db-migration.columns.exist' | translate }}
                  <p-sortIcon field="exist"></p-sortIcon>
                </th>
                <th pSortableColumn="pending" scope="col">
                  {{ 'db-migration.columns.pending' | translate }}
                  <p-sortIcon field="pending"></p-sortIcon>
                </th>
                <th style="width: 4rem" scope="col"></th>
              </tr>
            </ng-template>
            <ng-template let-migration pTemplate="body">
              <tr>
                <td>{{ migration.name }}</td>
                <td>
                  <p-tag
                    [severity]="migration.applied ? 'success' : 'secondary'"
                    [value]="(migration.applied ? 'db-migration.yes' : 'db-migration.no') | translate"></p-tag>
                </td>
                <td>
                  <p-tag
                    [severity]="migration.exist ? 'success' : 'secondary'"
                    [value]="(migration.exist ? 'db-migration.yes' : 'db-migration.no') | translate"></p-tag>
                </td>
                <td>
                  <p-tag
                    [severity]="migration.pending ? 'warn' : 'secondary'"
                    [value]="(migration.pending ? 'db-migration.yes' : 'db-migration.no') | translate"></p-tag>
                </td>
                <td>
                  <p-button
                    icon="pi pi-bolt"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="left"
                    [pTooltip]="'db-migration.actions.applyMigration' | translate"
                    (onClick)="applyMigration(migration)"></p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="5">{{ 'db-migration.empty' | translate }}</td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id" />
    }
  `,
  providers: [ConfirmationService]
})
export class DbMigrationComponent
  extends BaseModuleComponent<NoSettingsDto, NoSettingsDto>
  implements OnInit, OnDestroy
{
  private readonly translations = provideTranslations({ en, ru });

  @ViewChild('dt') dt!: Table;

  data: MigrationDto[] = [];
  protected loading = false;
  protected globalFilter = '';

  async ngOnInit() {
    await super.ngOnInit();
    await this.refreshAction();
  }

  async refreshAction() {
    this.loading = true;
    try {
      this.data = await this.getData();
    } catch (error) {
      console.log(error);
      this.msgService.error(this.l10nService.instant('db-migration.messages.errorRefreshing'));
    } finally {
      this.loading = false;
    }
  }

  protected get hasActiveFilters(): boolean {
    if (!this.dt?.filters) return false;
    return Object.values(this.dt.filters).some((meta) =>
      (Array.isArray(meta) ? meta : [meta]).some((m) => m?.value !== null && m?.value !== undefined && m?.value !== '')
    );
  }

  clearFilter() {
    this.globalFilter = '';
    this.dt.clearFilterValues();
    this.dt._filter();
  }

  async getData() {
    return this.getMigrations<MigrationDto>();
  }

  async applyMigration(rowData: MigrationDto) {
    const request = { name: rowData.name } as ApplyMigrationRequest;
    return this.applyModuleMigration(request);
  }
}
