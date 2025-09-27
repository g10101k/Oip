import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent } from 'oip-common';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { NgIf } from '@angular/common';
import { Tooltip } from 'primeng/tooltip';
import { ActivatedRoute } from '@angular/router';

export interface MigrationDto {
  name: string;
  applied: boolean;
  pending: boolean;
  exist: boolean;
}

export interface ApplyMigrationRequest {
  name: string;
}

interface DbMigrationSettingsDto {}

@Component({
  imports: [
    TableModule,
    SharedModule,
    TagModule,
    InputTextModule,
    TextareaModule,
    ButtonModule,
    FormsModule,
    ConfirmDialog,
    NgIf,
    SecurityComponent,
    Tooltip
  ],
  selector: 'crypt',
  template: `
    <div *ngIf="isContent" class="card" style="height: 100%">
      <p-confirmDialog />
      <div>
        <h5>Migration manager</h5>
        <div class="flex flex-row gap-2">
          <p-button
            icon="pi pi-refresh"
            pTooltip="Refresh"
            severity="secondary"
            tooltipPosition="bottom"
            [outlined]="true"
            (click)="refreshAction()" />
          <p-button
            icon="pi pi-filter-slash"
            pTooltip="Clean filter"
            severity="secondary"
            tooltipPosition="bottom"
            [outlined]="true"
            (click)="dt.clear()" />
        </div>
        <div>
          <p-table #dt dataKey="name" editMode="row" size="small" [scrollable]="true" [value]="data">
            <ng-template let-columns pTemplate="header">
              <tr>
                <th pSortableColumn="name" scope="col">
                  Migration name
                  <p-columnFilter display="menu" field="name" type="text" />
                </th>
                <th scope="col">Applied</th>
                <th scope="col">Exist</th>
                <th>Pending</th>
                <th scope="col"></th>
              </tr>
            </ng-template>

            <ng-template #body let-columns="columns" let-editing="editing" let-ri="rowIndex" let-rowData>
              <tr [pEditableRow]="rowData">
                <td>
                  {{ rowData.name }}
                </td>
                <td>
                  <p-button
                    *ngIf="rowData.applied"
                    icon="pi pi-check"
                    severity="success"
                    [rounded]="true"
                    [text]="true">
                  </p-button>
                </td>
                <td>
                  <p-button *ngIf="rowData.exist" icon="pi pi-check" severity="success" [rounded]="true" [text]="true">
                  </p-button>
                </td>
                <td>
                  <p-button
                    *ngIf="rowData.pending"
                    icon="pi pi-check"
                    severity="success"
                    [rounded]="true"
                    [text]="true">
                  </p-button>
                </td>
                <td>
                  <p-button
                    icon="pi pi-bolt"
                    pCancelEditableRow
                    pTooltip="Apply migration"
                    severity="secondary"
                    tooltipPosition="left"
                    [rounded]="true"
                    [text]="true"
                    (click)="applyMigration(rowData)">
                  </p-button>
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>
    </div>
    <security *ngIf="isSecurity" [controller]="controller" [id]="id"></security>
  `,
  providers: [ConfirmationService]
})
export class DbMigrationComponent extends BaseComponent<DbMigrationSettingsDto> implements OnInit, OnDestroy, Feature {
  private readonly activatedRoute = inject(ActivatedRoute);
  controller: string = '';
  data: MigrationDto[];

  async ngOnInit() {
    this.activatedRoute.url.subscribe((url) => {
      this.controller = url[0].toString();
    });
    this.titleService.setTitle('Db Migration');
    await super.ngOnInit();
    await this.refreshAction();
  }

  async refreshAction() {
    this.getData()
      .then((response) => {
        this.data = response;
      })
      .catch((error) => {
        console.log(error);
        this.msgService.error('Error refreshing database');
      });
  }

  async getData() {
    return this.baseDataService.sendRequest<MigrationDto[]>(`api/${this.controller}/get-migrations`, 'GET');
  }

  async applyMigration(rowData: MigrationDto) {
    const request = { name: rowData.name } as ApplyMigrationRequest;
    return this.baseDataService.sendRequest(`api/${this.controller}/apply-migration`, 'POST', request);
  }
}
