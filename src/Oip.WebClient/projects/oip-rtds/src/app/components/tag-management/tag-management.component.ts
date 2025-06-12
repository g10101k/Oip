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
import { InputNumber } from "primeng/inputnumber";
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

export interface WeatherSettingsDto {
}

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
                          optionLabel="label" optionValue="value"></p-dropdown>
            </div>

            <label for="source" class="flex items-center col-span-2 mb-2 md:col-span-2 md:mb-0">Source</label>
            <div class="col-span-4 md:col-span-4">
              <input class="w-full" pInputText id="source" [(ngModel)]="selectedTag.source"/>
            </div>
          </div>

          <!-- Группа: Описательные метаданные -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="descriptor" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Descriptor</label>
            <div class="col-span-12 md:col-span-10">
              <input class="w-full" pInputText id="descriptor" [(ngModel)]="selectedTag.descriptor"/>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="engUnits" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Engineering
              Units</label>
            <div class="col-span-12 md:col-span-10">
              <input pInputText id="engUnits" [(ngModel)]="selectedTag.engUnits"/>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="exDesc" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Extended
              Description</label>
            <div class="col-span-12 md:col-span-10">
              <textarea pInputTextarea id="exDesc" [(ngModel)]="selectedTag.exDesc" rows="3"></textarea>
            </div>
          </div>

          <!-- Группа: Конфигурация архивирования -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-4">
              <p-checkbox [(ngModel)]="selectedTag.archiving" binary="true" label="Archiving"></p-checkbox>
            </div>
            <div class="col-span-12 md:col-span-4">
              <label for="excDev" class="flex items-center mb-2">Exception Deviation</label>
              <p-inputNumber id="excDev" [(ngModel)]="selectedTag.excDev" mode="decimal"></p-inputNumber>
            </div>
            <div class="col-span-12 md:col-span-4">
              <label for="excMin" class="flex items-center mb-2">Exception Min (sec)</label>
              <p-inputNumber id="excMin" [(ngModel)]="selectedTag.excMin" mode="decimal"></p-inputNumber>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-4">
              <label for="excMax" class="flex items-center mb-2">Exception Max (sec)</label>
              <p-inputNumber id="excMax" [(ngModel)]="selectedTag.excMax" mode="decimal"></p-inputNumber>
            </div>
          </div>

          <!-- Группа: Конфигурация компрессии -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-4">
              <p-checkbox [(ngModel)]="selectedTag.compressing" binary="true" label="Compressing"></p-checkbox>
            </div>
            <div class="col-span-12 md:col-span-4">
              <label for="compDev" class="flex items-center mb-2">Compression Deviation</label>
              <p-inputNumber id="compDev" [(ngModel)]="selectedTag.compDev" mode="decimal"></p-inputNumber>
            </div>
            <div class="col-span-12 md:col-span-4">
              <label for="compMin" class="flex items-center mb-2">Compression Min (sec)</label>
              <p-inputNumber id="compMin" [(ngModel)]="selectedTag.compMin" mode="decimal"></p-inputNumber>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-4">
              <label for="compMax" class="flex items-center mb-2">Compression Max (sec)</label>
              <p-inputNumber id="compMax" [(ngModel)]="selectedTag.compMax" mode="decimal"></p-inputNumber>
            </div>
          </div>

          <!-- Группа: Калибровка/масштабирование -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-6">
              <label for="zero" class="flex items-center mb-2">Zero</label>
              <p-inputNumber id="zero" [(ngModel)]="selectedTag.zero" mode="decimal"></p-inputNumber>
            </div>
            <div class="col-span-12 md:col-span-6">
              <label for="span" class="flex items-center mb-2">Span</label>
              <p-inputNumber id="span" [(ngModel)]="selectedTag.span" mode="decimal"></p-inputNumber>
            </div>
          </div>

          <!-- Группа: Параметры интеграции -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="source" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Source</label>
            <div class="col-span-12 md:col-span-10">
              <input pInputText id="source" [(ngModel)]="selectedTag.source"/>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="instrumentTag" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Instrument
              Tag</label>
            <div class="col-span-12 md:col-span-10">
              <input pInputText id="instrumentTag" [(ngModel)]="selectedTag.instrumentTag"/>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-4">
              <p-checkbox [(ngModel)]="selectedTag.scan" binary="true" label="Scan"></p-checkbox>
            </div>
            <div class="col-span-12 md:col-span-4">
              <p-checkbox [(ngModel)]="selectedTag.step" binary="true" label="Step"></p-checkbox>
            </div>
            <div class="col-span-12 md:col-span-4">
              <p-checkbox [(ngModel)]="selectedTag.future" binary="true" label="Future"></p-checkbox>
            </div>
          </div>

          <!-- Location параметры -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <div *ngFor="let loc of [1,2,3,4,5]" class="col-span-12 md:col-span-2">
              <label [for]="'location' + loc" class="flex items-center mb-2">Location {{ loc }}</label>
              <p-inputNumber [id]="'location' + loc" [(ngModel)]="selectedTag['location' + loc]"
                             mode="decimal"></p-inputNumber>
            </div>
          </div>

          <!-- Пользовательские поля -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <div *ngFor="let num of [1,2,3,4,5]" class="col-span-12 md:col-span-2">
              <label [for]="'userInt' + num" class="flex items-center mb-2">User Int {{ num }}</label>
              <p-inputNumber [id]="'userInt' + num" [(ngModel)]="selectedTag['userInt' + num]"
                             mode="decimal"></p-inputNumber>
            </div>
          </div>

          <div class="grid grid-cols-12 gap-4 mt-4">
            <div *ngFor="let num of [1,2,3,4,5]" class="col-span-12 md:col-span-2">
              <label [for]="'userReal' + num" class="flex items-center mb-2">User Real {{ num }}</label>
              <p-inputNumber [id]="'userReal' + num" [(ngModel)]="selectedTag['userReal' + num]"
                             mode="decimal"></p-inputNumber>
            </div>
          </div>

          <!-- Аудит -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <div class="col-span-12 md:col-span-6">
              <label for="creationDate" class="flex items-center mb-2">Creation Date</label>
              <p-calendar id="creationDate" [(ngModel)]="selectedTag.creationDate" [showTime]="true"
                          dateFormat="yy-mm-dd"></p-calendar>
            </div>
            <div class="col-span-12 md:col-span-6">
              <label for="creator" class="flex items-center mb-2">Creator</label>
              <input pInputText id="creator" [(ngModel)]="selectedTag.creator"/>
            </div>
          </div>

          <!-- Конфигурация хранилища -->
          <div class="grid grid-cols-12 gap-4 mt-4">
            <label for="partition" class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0">Partition</label>
            <div class="col-span-12 md:col-span-10">
              <input pInputText id="partition" [(ngModel)]="selectedTag.partition"/>
            </div>
          </div>

          <div class="flex justify-end mt-4">
            <p-button label="Save" icon="pi pi-save" (onClick)="saveTag()"></p-button>
          </div>
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
    InputNumber,
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
    await this.saveSettings($event);
  }

  saveTag() {
    // Реализация сохранения тега
    console.log('Saving tag:', this.selectedTag);
    // Здесь должен быть вызов API для сохранения изменений
  }
}
