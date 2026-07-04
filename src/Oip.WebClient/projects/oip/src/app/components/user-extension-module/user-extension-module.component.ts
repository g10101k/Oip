import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { AfterViewInit, Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FilterMetadata, SharedModule } from 'primeng/api';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { BaseModuleComponent, SecurityComponent } from 'oip-common';
import { UserExtensionModuleApi } from '../../../api/user-extension-module.api';
import {
  CreateUserExtensionFieldRequest,
  ExtensionFieldMetadataDto,
  ExtensionFieldOptionDto,
  ExtensionFieldType,
  ExtensionTableColumnDto,
  TableQueryRequest,
  UserExtensionTableRowDto
} from '../../../api/data-contracts';

interface UserExtensionModuleSettings {
}

interface UserExtensionModuleLocalSettings {
  first?: number;
  rows?: number;
  sortField?: string | null;
  sortOrder?: number;
  globalFilter?: string;
  filters?: {
    [field: string]: FilterMetadata | FilterMetadata[];
  };
  columns?: Record<string, ColumnPresentationSetting>;
}

interface ColumnPresentationSetting {
  visible?: boolean;
  order?: number;
  width?: string;
  format?: string;
}

interface UserExtensionTableItem extends UserExtensionTableRowDto {
  _isEditing?: boolean;
  _editValues?: Record<string, unknown>;
}

interface SelectOption<TValue = string> {
  label: string;
  value: TValue;
}

@Component({
  template: `
    @if (isContent) {
      <div class="card space-y-4">
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h5 class="mb-1">{{ title || 'User extension table' }}</h5>
            <p class="m-0 text-surface-500">Users with dynamic extension fields</p>
          </div>
          <div class="flex flex-col gap-2 sm:flex-row">
            <input
              pInputText
              type="text"
              placeholder="Search users"
              [(ngModel)]="globalFilter"
              (keydown.enter)="applyGlobalFilter()"/>
            <p-button icon="pi pi-search" label="Search" (onClick)="applyGlobalFilter()"/>
            <p-button icon="pi pi-filter-slash" severity="secondary" label="Clear" (onClick)="clearFilters()"/>
          </div>
        </div>

        <p-table
          #table
          [value]="rows"
          [lazy]="true"
          [lazyLoadOnInit]="false"
          [loading]="loading"
          [paginator]="true"
          [rows]="localSettings().rows ?? 25"
          [first]="localSettings().first ?? 0"
          [totalRecords]="totalRecords"
          [rowsPerPageOptions]="[10, 25, 50, 100, 500]"
          [filters]="localSettings().filters ?? {}"
          [sortField]="localSettings().sortField ?? undefined"
          [sortOrder]="localSettings().sortOrder ?? 1"
          responsiveLayout="scroll"
          dataKey="userId"
          (onLazyLoad)="loadUsers($event)">
          <ng-template pTemplate="header">
            <tr>
              @for (column of visibleColumns; track column.field) {
                <th
                  [style.width]="column.width || undefined"
                  [pSortableColumn]="column.isSortable === false ? undefined : column.field || undefined">
                  <div class="flex min-w-32 items-center gap-2">
                    <span>{{ column.header || column.field }}</span>
                    @if (column.isSortable !== false) {
                      <p-sortIcon [field]="column.field || ''"/>
                    }
                    @if (column.isFilterable !== false) {
                      <p-columnFilter display="menu" [field]="column.field || ''" [type]="filterType(column)"/>
                    }
                  </div>
                </th>
              }
              <th class="w-32 text-center">Actions</th>
            </tr>
          </ng-template>

          <ng-template pTemplate="body" let-row>
            <tr>
              @for (column of visibleColumns; track column.field) {
                <td [ngClass]="cellClass(column)">
                  @if (row._isEditing && !column.isBase) {
                    @switch (column.type) {
                      @case (fieldTypes.Boolean) {
                        <input
                          type="checkbox"
                          [ngModel]="row._editValues?.[column.field || ''] === true"
                          (ngModelChange)="setEditValue(row, column, $event)"/>
                      }
                      @case (fieldTypes.Select) {
                        <p-select
                          appendTo="body"
                          class="w-full min-w-36"
                          optionLabel="label"
                          optionValue="value"
                          [options]="selectOptions(column)"
                          [ngModel]="row._editValues?.[column.field || '']"
                          (ngModelChange)="setEditValue(row, column, $event)"/>
                      }
                      @default {
                        <input
                          pInputText
                          class="w-full min-w-36"
                          [type]="inputType(column)"
                          [ngModel]="row._editValues?.[column.field || '']"
                          (ngModelChange)="setEditValue(row, column, $event)"/>
                      }
                    }
                  } @else {
                    @switch (column.type) {
                      @case (fieldTypes.Boolean) {
                        <p-tag
                          [value]="value(row, column) ? 'Yes' : 'No'"
                          [severity]="value(row, column) ? 'success' : 'secondary'"/>
                      }
                      @case (fieldTypes.Date) {
                        {{ value(row, column) | date: dateFormat(column) }}
                      }
                      @case (fieldTypes.DateTime) {
                        {{ value(row, column) | date: dateTimeFormat(column) }}
                      }
                      @case (fieldTypes.Number) {
                        <span class="block text-right tabular-nums">{{ value(row, column) | number: numberFormat(column) }}</span>
                      }
                      @case (fieldTypes.Select) {
                        @if (optionFor(column, value(row, column)); as option) {
                          <p-tag [value]="option.label || option.value || ''" [severity]="option.severity || 'info'"/>
                        } @else {
                          {{ value(row, column) }}
                        }
                      }
                      @default {
                        {{ value(row, column) }}
                      }
                    }
                  }
                </td>
              }
              <td>
                <div class="flex items-center justify-center gap-2">
                  @if (row._isEditing) {
                    <p-button icon="pi pi-check" severity="success" [text]="true" [disabled]="saving || !canEdit" (onClick)="saveRow(row)"/>
                    <p-button icon="pi pi-times" severity="secondary" [text]="true" [disabled]="saving" (onClick)="cancelEdit(row)"/>
                  } @else {
                    <p-button icon="pi pi-pencil" [text]="true" [disabled]="saving || !canEdit" (onClick)="editRow(row)"/>
                  }
                </div>
              </td>
            </tr>
          </ng-template>

          <ng-template pTemplate="emptymessage">
            <tr>
              <td [attr.colspan]="visibleColumns.length + 1" class="py-8 text-center text-surface-500">
                No users found
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    } @else if (isSettings) {
      <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(360px,420px)]">
        <div class="card space-y-4">
          <div>
            <h5 class="mb-1">Columns</h5>
            <p class="m-0 text-surface-500">Presentation is stored for this module instance</p>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full min-w-[720px] border-separate border-spacing-y-2">
              <thead>
                <tr class="text-left text-sm text-surface-500">
                  <th class="w-20">Visible</th>
                  <th>Column</th>
                  <th class="w-28">Order</th>
                  <th class="w-36">Width</th>
                  <th class="w-44">Format</th>
                </tr>
              </thead>
              <tbody>
                @for (column of columns; track column.field) {
                  <tr>
                    <td>
                      <input
                        type="checkbox"
                        [ngModel]="presentation(column).visible !== false"
                        (ngModelChange)="updateColumnSetting(column, { visible: $event })"/>
                    </td>
                    <td>
                      <div class="font-medium">{{ column.header || column.field }}</div>
                      <div class="text-xs text-surface-500">{{ column.isBase ? 'Base user field' : column.type }}</div>
                    </td>
                    <td>
                      <input
                        pInputText
                        type="number"
                        class="w-full"
                        [ngModel]="presentation(column).order ?? column.order ?? 0"
                        (ngModelChange)="updateColumnSetting(column, { order: toNumber($event) })"/>
                    </td>
                    <td>
                      <input
                        pInputText
                        class="w-full"
                        placeholder="12rem"
                        [ngModel]="presentation(column).width ?? column.width ?? ''"
                        (ngModelChange)="updateColumnSetting(column, { width: $event })"/>
                    </td>
                    <td>
                      <input
                        pInputText
                        class="w-full"
                        placeholder="1.2-2"
                        [ngModel]="presentation(column).format ?? column.format ?? ''"
                        (ngModelChange)="updateColumnSetting(column, { format: $event })"/>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        <div class="card space-y-4">
          <div>
            <h5 class="mb-1">Global fields</h5>
            <p class="m-0 text-surface-500">Schema fields are shared for all module instances</p>
          </div>
          <div class="grid gap-3">
            <input pInputText placeholder="Field name" [(ngModel)]="newField.fieldName"/>
            <input pInputText placeholder="Database column" [(ngModel)]="newField.dbColumn"/>
            <p-select
              optionLabel="label"
              optionValue="value"
              [options]="fieldTypeOptions"
              [(ngModel)]="newField.type"/>
            <textarea
              class="min-h-24 rounded-md border border-surface-300 bg-transparent p-3"
              placeholder="Options: value|label|severity"
              [(ngModel)]="newOptionsText"></textarea>
            <div class="grid grid-cols-2 gap-3 text-sm">
              <label class="flex items-center gap-2"><input type="checkbox" [(ngModel)]="newField.isVisible"/> Visible</label>
              <label class="flex items-center gap-2"><input type="checkbox" [(ngModel)]="newField.isRequired"/> Required</label>
              <label class="flex items-center gap-2"><input type="checkbox" [(ngModel)]="newField.isSortable"/> Sortable</label>
              <label class="flex items-center gap-2"><input type="checkbox" [(ngModel)]="newField.isFilterable"/> Filterable</label>
            </div>
            <input pInputText type="number" placeholder="Order" [(ngModel)]="newField.order"/>
            <p-button icon="pi pi-plus" label="Create field" [disabled]="saving || !canEdit" (onClick)="createField()"/>
          </div>

          <div class="space-y-2">
            @for (field of extensionFields; track field.id) {
              <div class="flex items-center justify-between gap-3 rounded-md border border-surface-200 p-3">
                <div>
                  <div class="font-medium">{{ field.fieldName }}</div>
                  <div class="text-xs text-surface-500">{{ field.dbColumn }} · {{ field.type }}</div>
                </div>
                <p-button
                  icon="pi pi-trash"
                  severity="danger"
                  [text]="true"
                  [disabled]="saving || !canDelete || !field.id"
                  (onClick)="deleteField(field)"/>
              </div>
            }
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `,
  providers: [UserExtensionModuleApi],
  imports: [
    TableModule,
    SharedModule,
    TagModule,
    SecurityComponent,
    Button,
    FormsModule,
    InputText,
    Select,
    DatePipe,
    DecimalPipe,
    NgClass
  ]
})
export class UserExtensionModuleComponent
  extends BaseModuleComponent<UserExtensionModuleSettings, UserExtensionModuleLocalSettings>
  implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('table') table!: Table;

  protected readonly dataService = inject(UserExtensionModuleApi);
  protected readonly fieldTypes = ExtensionFieldType;
  protected readonly fieldTypeOptions: SelectOption<ExtensionFieldType>[] = Object.values(ExtensionFieldType).map((type) => ({
    label: type,
    value: type
  }));

  protected columns: ExtensionTableColumnDto[] = [];
  protected visibleColumns: ExtensionTableColumnDto[] = [];
  protected extensionFields: ExtensionFieldMetadataDto[] = [];
  protected rows: UserExtensionTableItem[] = [];
  protected totalRecords = 0;
  protected loading = false;
  protected saving = false;
  protected globalFilter = '';
  protected newOptionsText = '';
  protected newField: CreateUserExtensionFieldRequest = this.createEmptyField();

  private viewInitialized = false;

  constructor() {
    super();
    this.appTitleService.setTitle('User extension table');
  }

  override async ngOnInit(): Promise<void> {
    await super.ngOnInit();
  }

  ngAfterViewInit(): void {
    this.viewInitialized = true;
    if (this.id !== undefined) {
      void this.loadUsers(this.createLazyEventFromState());
    }
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    this.globalFilter = this.localSettings().globalFilter ?? '';
    await this.loadFields();
    if (this.viewInitialized) {
      await this.loadUsers(this.createLazyEventFromState());
    }
  }

  protected async loadUsers(event: TableLazyLoadEvent): Promise<void> {
    if (this.securityRightsLoaded && !this.canRead) {
      this.rows = [];
      this.totalRecords = 0;
      return;
    }

    this.loading = true;
    const filters = (event.filters ?? this.localSettings().filters ?? {}) as {
      [field: string]: FilterMetadata | FilterMetadata[];
    };
    const request: TableQueryRequest = {
      first: event.first ?? this.localSettings().first ?? 0,
      rows: event.rows ?? this.localSettings().rows ?? 25,
      sortField: typeof event.sortField === 'string' ? event.sortField : this.localSettings().sortField ?? undefined,
      sortOrder: event.sortOrder ?? this.localSettings().sortOrder ?? 1,
      globalFilter: this.globalFilter,
      filters: filters as never
    };

    try {
      const response = await this.dataService.getUserExtensionPage(request);
      this.columns = response.columns ?? this.columns;
      this.syncVisibleColumns();
      this.rows = (response.data ?? []).map((row) => ({...row}));
      this.totalRecords = response.total ?? 0;
      this.localSettings.set({
        ...this.localSettings(),
        first: request.first ?? 0,
        rows: request.rows ?? 25,
        sortField: request.sortField ?? null,
        sortOrder: request.sortOrder ?? 1,
        globalFilter: this.globalFilter,
        filters
      });
    } catch (error) {
      this.rows = [];
      this.totalRecords = 0;
      this.msgService.errorFromException(error, 'Load user extension table error');
    } finally {
      this.loading = false;
    }
  }

  protected applyGlobalFilter(): void {
    this.table.filterGlobal(this.globalFilter, 'contains');
  }

  protected clearFilters(): void {
    this.globalFilter = '';
    this.table.clear();
  }

  protected editRow(row: UserExtensionTableItem): void {
    if (!this.canEdit) {
      return;
    }

    for (const item of this.rows) {
      item._isEditing = false;
      item._editValues = undefined;
    }

    row._isEditing = true;
    row._editValues = {...(row.extensionValues ?? {})};
  }

  protected cancelEdit(row: UserExtensionTableItem): void {
    row._isEditing = false;
    row._editValues = undefined;
  }

  protected setEditValue(row: UserExtensionTableItem, column: ExtensionTableColumnDto, value: unknown): void {
    if (!column.field) {
      return;
    }

    row._editValues = {
      ...(row._editValues ?? {}),
      [column.field]: value
    };
  }

  protected async saveRow(row: UserExtensionTableItem): Promise<void> {
    if (!this.canEdit || !row.userId) {
      return;
    }

    this.saving = true;
    try {
      await this.dataService.updateUserExtensionValues(
        {userId: row.userId},
        {values: row._editValues ?? {}}
      );
      row._isEditing = false;
      row._editValues = undefined;
      this.msgService.success('User extension values saved');
      await this.loadUsers(this.createLazyEventFromState());
    } catch (error) {
      this.msgService.errorFromException(error, 'Save user extension values error');
    } finally {
      this.saving = false;
    }
  }

  protected async createField(): Promise<void> {
    if (!this.canEdit) {
      return;
    }

    this.saving = true;
    try {
      await this.dataService.createUserExtensionField({
        ...this.newField,
        options: this.parseOptions(this.newOptionsText)
      });
      this.newField = this.createEmptyField();
      this.newOptionsText = '';
      this.msgService.success('Extension field created');
      await this.loadFields();
      await this.loadUsers(this.createLazyEventFromState());
    } catch (error) {
      this.msgService.errorFromException(error, 'Create extension field error');
    } finally {
      this.saving = false;
    }
  }

  protected async deleteField(field: ExtensionFieldMetadataDto): Promise<void> {
    if (!this.canDelete || !field.id || !window.confirm(`Delete field ${field.fieldName}?`)) {
      return;
    }

    this.saving = true;
    try {
      await this.dataService.deleteUserExtensionField({id: field.id});
      this.msgService.success('Extension field deleted');
      await this.loadFields();
      await this.loadUsers(this.createLazyEventFromState());
    } catch (error) {
      this.msgService.errorFromException(error, 'Delete extension field error');
    } finally {
      this.saving = false;
    }
  }

  protected value(row: UserExtensionTableItem, column: ExtensionTableColumnDto): unknown {
    if (!column.field) {
      return null;
    }

    return column.isBase
      ? (row as Record<string, unknown>)[column.field]
      : row.extensionValues?.[column.field];
  }

  protected presentation(column: ExtensionTableColumnDto): ColumnPresentationSetting {
    if (!column.field) {
      return {};
    }

    return this.localSettings().columns?.[column.field] ?? {};
  }

  protected updateColumnSetting(column: ExtensionTableColumnDto, patch: ColumnPresentationSetting): void {
    if (!column.field) {
      return;
    }

    const columns = {...(this.localSettings().columns ?? {})};
    columns[column.field] = {
      ...(columns[column.field] ?? {}),
      ...patch
    };

    this.localSettings.set({
      ...this.localSettings(),
      columns
    });
    this.syncVisibleColumns();
  }

  protected filterType(column: ExtensionTableColumnDto): 'text' | 'numeric' | 'date' | 'boolean' {
    switch (column.type) {
      case ExtensionFieldType.Number:
        return 'numeric';
      case ExtensionFieldType.Boolean:
        return 'boolean';
      case ExtensionFieldType.Date:
      case ExtensionFieldType.DateTime:
        return 'date';
      default:
        return 'text';
    }
  }

  protected inputType(column: ExtensionTableColumnDto): string {
    switch (column.type) {
      case ExtensionFieldType.Number:
        return 'number';
      case ExtensionFieldType.Date:
        return 'date';
      case ExtensionFieldType.DateTime:
        return 'datetime-local';
      default:
        return 'text';
    }
  }

  protected cellClass(column: ExtensionTableColumnDto): string {
    return column.type === ExtensionFieldType.Number ? 'text-right' : '';
  }

  protected selectOptions(column: ExtensionTableColumnDto): SelectOption[] {
    return (column.options ?? []).map((option) => ({
      label: option.label ?? option.value ?? '',
      value: option.value ?? ''
    }));
  }

  protected optionFor(column: ExtensionTableColumnDto, value: unknown): ExtensionFieldOptionDto | undefined {
    return (column.options ?? []).find((option) => option.value === String(value ?? ''));
  }

  protected numberFormat(column: ExtensionTableColumnDto): string {
    return this.presentation(column).format || column.format || '1.0-2';
  }

  protected dateFormat(column: ExtensionTableColumnDto): string {
    return this.presentation(column).format || column.format || this.layoutService.dateFormat();
  }

  protected dateTimeFormat(column: ExtensionTableColumnDto): string {
    return this.presentation(column).format || column.format || this.layoutService.dateTimeFormat();
  }

  protected toNumber(value: unknown): number {
    return Number(value) || 0;
  }

  private createLazyEventFromState(): TableLazyLoadEvent {
    return {
      first: this.localSettings().first ?? 0,
      rows: this.localSettings().rows ?? 25,
      sortField: this.localSettings().sortField ?? undefined,
      sortOrder: this.localSettings().sortOrder ?? 1,
      filters: this.localSettings().filters ?? {}
    };
  }

  private async loadFields(): Promise<void> {
    try {
      this.extensionFields = await this.dataService.getUserExtensionFields();
    } catch (error) {
      this.extensionFields = [];
      this.msgService.errorFromException(error, 'Load extension field metadata error');
    }
  }

  private syncVisibleColumns(): void {
    this.visibleColumns = this.columns
      .map((column) => ({
        ...column,
        ...this.presentation(column)
      }))
      .filter((column) => column.isVisible !== false && this.presentation(column).visible !== false)
      .sort((left, right) => (left.order ?? 0) - (right.order ?? 0));
  }

  private parseOptions(text: string): ExtensionFieldOptionDto[] {
    return text
      .split('\n')
      .map((line) => line.trim())
      .filter((line) => line.length > 0)
      .map((line) => {
        const [value, label, severity] = line.split('|').map((part) => part.trim());
        return {value, label: label || value, severity: severity || null};
      });
  }

  private createEmptyField(): CreateUserExtensionFieldRequest {
    return {
      fieldName: '',
      dbColumn: '',
      type: ExtensionFieldType.Text,
      options: [],
      isRequired: false,
      isVisible: true,
      isSortable: true,
      isFilterable: true,
      order: 100
    };
  }
}
