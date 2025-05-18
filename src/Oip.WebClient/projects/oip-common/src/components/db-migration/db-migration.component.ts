import { Component, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent } from 'oip-common'
import { TagModule } from 'primeng/tag';
import { ConfirmationService, SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { ConfirmDialog } from "primeng/confirmdialog";
import { NgIf } from "@angular/common";
import { Tooltip } from "primeng/tooltip";

export interface MigrationDto {
  name: string,
  applied: boolean,
  pending: boolean,
  exist: boolean,
}

export interface ApplyMigrationRequest {
  name: string,
}

interface DbMigrationSettingsDto {
}

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
    Tooltip,
  ],
  selector: 'crypt',
  template: `
    <div *ngIf="isContent" class="card" style="height: 100%">
      <p-confirmDialog/>
      <div>
        <h5>Migration manager</h5>
        <div class="flex flex-row gap-2">
          <p-button icon="pi pi-refresh"
                    severity="secondary"
                    pTooltip="Refresh"
                    tooltipPosition="bottom"
                    [outlined]="true"
                    (click)="refreshAction()"/>
          <p-button icon="pi pi-filter-slash"
                    severity="secondary"
                    [outlined]="true"
                    pTooltip="Clean filter"
                    tooltipPosition="bottom"
                    (click)="dt.clear()"/>
        </div>
        <div>
          <p-table #dt
                   [value]="data"
                   dataKey="name"
                   editMode="row"
                   [scrollable]="true"
                   size="small">
            <ng-template pTemplate="header" let-columns>
              <tr>
                <th pSortableColumn="name" scope="col">
                  Migration name
                  <p-columnFilter type="text" field="name" display="menu"/>
                </th>
                <th scope="col">
                  Applied
                </th>
                <th scope="col">
                  Exist
                </th>
                <th> Pending</th>
                <th scope="col"></th>
              </tr>
            </ng-template>

            <ng-template #body let-editing="editing" let-ri="rowIndex" let-rowData let-columns="columns">
              <tr [pEditableRow]="rowData">
                <td>
                  {{ rowData.name }}
                </td>
                <td>
                  <p-button *ngIf="rowData.applied"
                            icon="pi pi-check"
                            severity="success"
                            [text]="true"
                            [rounded]="true">
                  </p-button>
                </td>
                <td>
                  <p-button *ngIf="rowData.exist"
                            icon="pi pi-check"
                            severity="success"
                            [text]="true"
                            [rounded]="true">
                  </p-button>
                </td>
                <td>
                  <p-button *ngIf="rowData.pending"
                            icon="pi pi-check"
                            severity="success"
                            [text]="true"
                            [rounded]="true">
                  </p-button>
                </td>
                <td>
                  <p-button icon="pi pi-bolt"
                            severity="secondary"
                            pCancelEditableRow
                            [text]="true"
                            [rounded]="true"
                            pTooltip="Apply migration"
                            tooltipPosition="left"
                            (click)="applyMigration(rowData)">
                  </p-button>
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>
    </div>
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
  `,
  providers: [ConfirmationService],
})
export class DbMigrationComponent extends BaseComponent<DbMigrationSettingsDto> implements OnInit, OnDestroy, Feature {
  controller: string = 'db-migration'
  data: MigrationDto[];

  async ngOnInit() {
    this.titleService.setTitle('Db Migration');
    await super.ngOnInit();
    await this.refreshAction();
  }

  async refreshAction() {
    this.getData().then((response) => {
      this.data = response;
    })
  }

  async getData() {
    return this.baseDataService.sendRequest<MigrationDto[]>(`api/${this.controller}/get-migrations`, 'GET');
  }

  async applyMigration(rowData: MigrationDto) {
    let request = { name: rowData.name } as ApplyMigrationRequest;
    return this.baseDataService.sendRequest(`api/${this.controller}/apply-migration`, 'POST', request);
  }
}
