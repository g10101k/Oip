import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { BaseComponent, Feature, SecurityComponent } from 'oip-common'
import { TagModule } from 'primeng/tag';
import { SharedModule } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { DatePipe, NgForOf, NgIf } from '@angular/common';
import { TagManagementModule } from "../../api/TagManagementModule";
import { DropdownModule } from "primeng/dropdown";
import { Checkbox } from "primeng/checkbox";
import { Calendar } from "primeng/calendar";
import { Button } from "primeng/button";
import { InputText } from "primeng/inputtext";
import { TagEntity } from "../../api/data-contracts";
import { FormsModule } from "@angular/forms";

export enum TagTypes {
  Float32 = 0,
  Float64 = 1,
  Int16 = 2,
  Int32 = 3,
  Digital = 4,
  String = 5,
  Blob = 6
}
export interface WeatherSettingsDto {}
@Component({
  selector: 'weather',
  template: `
    <div *ngIf="isContent">
      <div class="grid grid-cols-12 gap-4">
        <div class="card col-span-12 xl:col-span-12">
          <div class="font-semibold text-xl">Tags</div>
          <p-table [value]="tags"
                   [paginator]="true"
                   [rows]="10"
                   [sortMode]="'multiple'"
                   [size]="'small'"
                   selectionMode="single"
                   [(selection)]="selectedTag"
                   responsiveLayout="scroll">
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="tagId">ID
                  <p-sortIcon field="tagId"/>
                </th>
                <th pSortableColumn="name">Name
                  <p-sortIcon field="name"/>
                </th>
                <th pSortableColumn="valueType">Type
                  <p-sortIcon field="valueType"/>
                </th>
                <th pSortableColumn="source">Source
                  <p-sortIcon field="source"/>
                </th>
                <th pSortableColumn="engUnits">Units
                  <p-sortIcon field="engUnits"/>
                </th>
                <th pSortableColumn="scan">Scan
                  <p-sortIcon field="scan"/>
                </th>
                <th pSortableColumn="archiving">Arch.
                  <p-sortIcon field="archiving"/>
                </th>
                <th pSortableColumn="compressing">Comp.
                  <p-sortIcon field="compressing"/>
                </th>
                <th pSortableColumn="creationDate">Created
                  <p-sortIcon field="creationDate"/>
                </th>
                <th>Actions</th>
              </tr>
            </ng-template>

            <ng-template pTemplate="body" let-tag>
              <tr [pSelectableRow]="tag">
                <td>{{ tag.tagId }}</td>
                <td>{{ tag.name }}</td>
                <td>{{ getTagTypeName(tag.valueType) }}</td>
                <td>{{ tag.source }}</td>
                <td>{{ tag.engUnits }}</td>
                <td>
                  <p-tag [value]="tag.scan ? 'Yes' : 'No'" [severity]="tag.scan ? 'success' : 'danger'"/>
                </td>
                <td>
                  <p-tag [value]="tag.archiving ? 'Yes' : 'No'" [severity]="tag.archiving ? 'info' : 'warn'"/>
                </td>
                <td>
                  <p-tag [value]="tag.compressing ? 'Yes' : 'No'" [severity]="tag.compressing ? 'info' : 'warn'"/>
                </td>
                <td>{{ tag.creationDate | date: 'yyyy-MM-dd HH:mm' }}</td>
                <td>
                  <p-button label="Save" type="submit" class="p-mt-3"></p-button>
                </td>
              </tr>
            </ng-template>

            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="10">No tags found.</td>
              </tr>
            </ng-template>
          </p-table>
        </div>

        <div *ngIf="selectedTag" class="card col-span-6 xl:col-span-6">
          <div class="font-semibold text-xl">Common</div>
          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="name" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Name</label>
            <div class="col-span-12 md:col-span-10">
              <input class="w-full" pInputText id="name" [(ngModel)]="selectedTag.name"/>
            </div>
          </div>
          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="valueType" class="flex items-center col-span-2 mb-2 md:col-span-2 md:mb-0">Value Type</label>
            <div class="col-span-4 md:col-span-4">
              <p-dropdown class="w-full" id="valueType" [options]="valueTypeOptions"
                          [(ngModel)]="selectedTag.valueType"></p-dropdown>
            </div>

            <label for="source" class="flex items-center col-span-2 mb-2 md:col-span-2 md:mb-0">Source</label>
            <div class="col-span-4 md:col-span-4">
              <input class="w-full" pInputText id="source" [(ngModel)]="selectedTag.source"/>
            </div>
          </div>
          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="descriptor" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Descriptor</label>
            <div class="col-span-12 md:col-span-10">
              <input class="w-full" pInputText id="descriptor" [(ngModel)]="selectedTag.descriptor"/>
            </div>
          </div>
          <div class="flex justify-end mt-4">
            <p-button label="Save" icon="pi pi-save"></p-button>
          </div>
        </div>

        <div *ngIf="selectedTag" class="flex flex-col md:flex-row gap-8">

          <div class="p-grid p-formgrid">


            <div class="p-col-12 p-md-6">
              <label for="engUnits">Eng Units</label>
              <input pInputText id="engUnits" [(ngModel)]="selectedTag.engUnits"/>
            </div>

            <div class="p-col-12 p-md-6">
              <label for="instrumentTag">Instrument Tag</label>
              <input pInputText id="instrumentTag" [(ngModel)]="selectedTag.instrumentTag"/>
            </div>

            <div class="p-col-12 p-md-6">
              <p-checkbox [(ngModel)]="selectedTag.archiving" binary="true" label="Archiving"></p-checkbox>
              <p-checkbox [(ngModel)]="selectedTag.compressing" binary="true" label="Compressing"></p-checkbox>
              <p-checkbox [(ngModel)]="selectedTag.scan" binary="true" label="Scan"></p-checkbox>
              <p-checkbox [(ngModel)]="selectedTag.step" binary="true" label="Step"></p-checkbox>
            </div>

            <div class="p-col-12 p-md-6">
              <label for="digitalSet">Digital Set</label>
              <input pInputText id="digitalSet" [(ngModel)]="selectedTag.digitalSet"/>
            </div>

            <div class="p-col-12 p-md-12">
              <label for="exDesc">Description</label>
              <textarea pInputTextarea id="exDesc" [ngModel]="selectedTag.exDesc" rows="3"></textarea>
            </div>


            <ng-container *ngFor="let field of ['excDev', 'excMin', 'excMax', 'compDev', 'compMin', 'compMax', 'zero', 'span',
                                           'location1', 'location2', 'location3', 'location4', 'location5',
                                           'userInt1', 'userInt2', 'userInt3', 'userInt4', 'userInt5',
                                           'userReal1', 'userReal2', 'userReal3', 'userReal4', 'userReal5']">
              <div class="p-col-12 p-md-4">
                <label [for]="field">{{ field }}</label>
                <input pInputNumber [inputId]="field" [formControlName]="field" mode="decimal"/>
              </div>
            </ng-container>

            <div class="p-col-12 p-md-6">
              <label for="creationDate">Creation Date</label>
              <p-calendar [ngModel]="selectedTag.creationDate" [showTime]="true" dateFormat="yy-mm-dd"></p-calendar>
            </div>

            <div class="p-col-12 p-md-6">
              <label for="creator">Creator</label>
              <input pInputText id="creator" [ngModel]="selectedTag.creator"/>
            </div>

            <div class="p-col-12">
              <label for="partition">Partition</label>
              <input pInputText id="partition" [ngModel]="selectedTag.partition"/>
            </div>
          </div>

          <p-button label="Save" type="submit" class="p-mt-3"></p-button>
        </div>
      </div>
    </div>

    <div *ngIf="isSettings"></div>
    <security *ngIf="isSecurity" [id]="id" [controller]="controller"></security>
  `,
  providers: [TagManagementModule],
  standalone: true,
  imports: [
    NgIf,
    TableModule,
    SharedModule,
    TagModule,
    SecurityComponent,
    DropdownModule,
    Checkbox,
    Calendar,
    Button,
    InputText,
    DatePipe,
    FormsModule,
    NgForOf,
  ],
})
export class TagManagement extends BaseComponent<WeatherSettingsDto> implements OnInit, OnDestroy, Feature {
  controller: string = 'tag-management';
  tagManagementModuleDataService = inject(TagManagementModule);
  tags: TagEntity[] = [];
  selectedTag: TagEntity;

  valueTypeOptions = [
    { label: 'Float32', value: 0 },
    { label: 'Float64', value: 1 },
    { label: 'Int16', value: 2 },
    { label: 'Int32', value: 3 },
    { label: 'Digital', value: 4 },
    { label: 'String', value: 5 },
    { label: 'Blob', value: 6 },
  ];

  getTagTypeName(type: TagTypes): string {
    return TagTypes[type];
  }

  async ngOnInit() {
    await super.ngOnInit();
    this.tags = await this.tagManagementModuleDataService.tagManagementGetTagsByFilterList({ filter: '%' });
  }

  async onSettingsChange($event: WeatherSettingsDto) {
    await this.saveSettings($event)
  }
}
