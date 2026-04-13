import { DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { AfterViewInit, Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FilterMetadata, SharedModule } from 'primeng/api';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { BaseModuleComponent, ContentType, L10nService, SecurityComponent } from 'oip-common';
import { CustomerModuleApi } from '../../../api/customer-module.api';
import { CustomerModuleSettings, DemoCustomerStatus, DemoCustomerTableRowDto, TableQueryRequest } from '../../../api/data-contracts';

interface CustomerModuleLocalSettings {
  first?: number;
  rows?: number;
  sortField?: string | null;
  sortOrder?: number;
  globalFilter?: string;
  filters?: {
    [field: string]: FilterMetadata | FilterMetadata[];
  };
}

interface CustomerEditModel {
  fullName: string;
  email: string;
  category: string;
  country: string;
  status: DemoCustomerStatus;
  creditScore: number;
  lifetimeValue: number;
}

interface CustomerTableItem extends DemoCustomerTableRowDto {
  _isEditing?: boolean;
  _isNew?: boolean;
  _editModel?: CustomerEditModel;
}

@Component({
  template: `
    @if (isContent) {
      <div class="card space-y-4">
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <h5 class="mb-1">{{ title }}</h5>
            <p class="m-0 text-surface-500">{{ 'customer-module.content.subtitle' | translate }}</p>
          </div>
          <div class="flex flex-col gap-2 sm:flex-row">
            <input
              pInputText
              type="text"
              [placeholder]="'customer-module.content.globalSearch' | translate"
              [(ngModel)]="globalFilter"
              (keydown.enter)="applyGlobalFilter()"/>
            <p-button
              icon="pi pi-search"
              [label]="'customer-module.content.search' | translate"
              (onClick)="applyGlobalFilter()"/>
            <p-button
              icon="pi pi-filter-slash"
              severity="secondary"
              [label]="'customer-module.content.clear' | translate"
              (onClick)="clearFilters()"/>
            <p-button
              icon="pi pi-plus"
              severity="success"
              [disabled]="loading"
              [label]="'customer-module.content.add' | translate"
              (onClick)="beginCreateCustomer()"/>
          </div>
        </div>

        <p-table
          #table
          [value]="visibleCustomers"
          [lazy]="true"
          [lazyLoadOnInit]="false"
          [loading]="loading"
          [paginator]="true"
          [rows]="localSettings().rows ?? 10"
          [first]="localSettings().first ?? 0"
          [totalRecords]="tableTotalRecords"
          [rowsPerPageOptions]="[10, 50, 100, 500, 1000, { showAll: 'customer-module.content.rowsPerPageOptionsAll' | translate }]"
          [filters]="localSettings().filters ?? {}"
          [sortField]="localSettings().sortField ?? undefined"
          [sortOrder]="localSettings().sortOrder ?? 1"
          [globalFilterFields]="globalFilterFields"
          responsiveLayout="scroll"
          dataKey="id"
          (onLazyLoad)="loadCustomers($event)">
          <ng-template pTemplate="header">
            <tr>
              <th pSortableColumn="fullName">
                {{ 'customer-module.content.table.fullName' | translate }}
                <p-sortIcon field="fullName"/>
                <p-columnFilter display="menu" field="fullName" type="text"/>
              </th>
              <th pSortableColumn="email">
                {{ 'customer-module.content.table.email' | translate }}
                <p-sortIcon field="email"/>
                <p-columnFilter display="menu" field="email" type="text"/>
              </th>
              <th pSortableColumn="categoryName">
                {{ 'customer-module.content.table.category' | translate }}
                <p-sortIcon field="categoryName"/>
                <p-columnFilter display="menu" field="categoryName" type="text"/>
              </th>
              <th pSortableColumn="countryName">
                {{ 'customer-module.content.table.country' | translate }}
                <p-sortIcon field="countryName"/>
                <p-columnFilter display="menu" field="countryName" type="text"/>
              </th>
              <th pSortableColumn="status">
                {{ 'customer-module.content.table.status' | translate }}
                <p-sortIcon field="status"/>
                <p-columnFilter display="menu" field="status" type="text"/>
              </th>
              <th pSortableColumn="creditScore">
                {{ 'customer-module.content.table.creditScore' | translate }}
                <p-sortIcon field="creditScore"/>
                <p-columnFilter display="menu" field="creditScore" type="numeric"/>
              </th>
              <th pSortableColumn="lifetimeValue">
                {{ 'customer-module.content.table.lifetimeValue' | translate }}
                <p-sortIcon field="lifetimeValue"/>
                <p-columnFilter display="menu" field="lifetimeValue" type="numeric"/>
              </th>
              <th pSortableColumn="createdAt">
                {{ 'customer-module.content.table.createdAt' | translate }}
                <p-sortIcon field="createdAt"/>
                <p-columnFilter display="menu" field="createdAt" type="date"/>
              </th>
              <th class="text-center">
                {{ 'customer-module.content.table.ordersCount' | translate }}
              </th>
              <th class="text-center min-w-44">
                {{ 'customer-module.content.table.actions' | translate }}
              </th>
            </tr>
          </ng-template>

          <ng-template pTemplate="body" let-customer>
            <tr>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <input pInputText class="w-full min-w-40" [(ngModel)]="editModel.fullName"/>
                } @else {
                  <span class="font-medium">{{ customer.fullName }}</span>
                }
              </td>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <input pInputText class="w-full min-w-40" [(ngModel)]="editModel.email"/>
                } @else {
                  {{ customer.email }}
                }
              </td>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <input pInputText class="w-full min-w-32" [(ngModel)]="editModel.category"/>
                } @else {
                  {{ customer.category }}
                }
              </td>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <input pInputText class="w-full min-w-32" [(ngModel)]="editModel.country"/>
                } @else {
                  {{ customer.country }}
                }
              </td>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <p-select
                    class="w-full min-w-32"
                    optionLabel="label"
                    optionValue="value"
                    [options]="statusOptions"
                    [(ngModel)]="editModel.status"/>
                } @else {
                  <p-tag [value]="customer.status" [severity]="statusSeverity(customer.status)"/>
                }
              </td>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <input pInputText type="number" class="w-full min-w-28 text-right" [(ngModel)]="editModel.creditScore"/>
                } @else {
                  <span class="block text-right tabular-nums">{{ customer.creditScore }}</span>
                }
              </td>
              <td>
                @if (customer._isEditing && customer._editModel; as editModel) {
                  <input pInputText type="number" class="w-full min-w-32 text-right" [(ngModel)]="editModel.lifetimeValue"/>
                } @else {
                  <span class="block text-right tabular-nums">{{ customer.lifetimeValue | number: '1.2-2' }}</span>
                }
              </td>
              <td>{{ customer.createdAt | date: layoutService.dateTimeFormat() }}</td>
              <td class="text-center">
                <span
                  class="inline-flex min-w-9 items-center justify-center rounded-full px-2 py-1 text-xs font-semibold"
                  [ngClass]="customer.ordersCount ? 'bg-green-100 text-green-800' : 'bg-surface-200 text-surface-700'">
                  {{ customer.ordersCount }}
                </span>
              </td>
              <td>
                <div class="flex items-center justify-center gap-2">
                  @if (customer._isEditing) {
                    <p-button
                      icon="pi pi-check"
                      severity="success"
                      [disabled]="activeRowAction"
                      [text]="true"
                      (onClick)="saveCustomer(customer)"/>
                    <p-button
                      icon="pi pi-times"
                      severity="secondary"
                      [disabled]="activeRowAction"
                      [text]="true"
                      (onClick)="cancelEdit(customer)"/>
                  } @else {
                    <p-button
                      icon="pi pi-pencil"
                      [disabled]="activeRowAction"
                      [text]="true"
                      (onClick)="editCustomer(customer)"/>
                    <p-button
                      icon="pi pi-trash"
                      severity="danger"
                      [disabled]="activeRowAction"
                      [text]="true"
                      (onClick)="deleteCustomer(customer)"/>
                  }
                </div>
              </td>
            </tr>
          </ng-template>

          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="10" class="py-8 text-center text-surface-500">
                {{ 'customer-module.content.empty' | translate }}
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    } @else if (isSettings) {
      <div class="card">
        <div class="font-semibold text-xl">{{ 'customer-module.settings.title' | translate }}</div>
        <p class="mt-3 mb-0 text-surface-500">{{ 'customer-module.settings.description' | translate }}</p>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `,
  providers: [CustomerModuleApi],
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
    TranslatePipe,
    NgClass
  ]
})
export class CustomerModuleComponent
  extends BaseModuleComponent<CustomerModuleSettings, CustomerModuleLocalSettings>
  implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('table') table!: Table;

  protected readonly dataService = inject(CustomerModuleApi);
  protected readonly l10nService = inject(L10nService);

  protected readonly globalFilterFields = ['fullName', 'email', 'categoryName', 'countryName'];
  protected readonly statusOptions = Object.values(DemoCustomerStatus).map((status) => ({
    label: status,
    value: status
  }));

  protected customers: CustomerTableItem[] = [];
  protected visibleCustomers: CustomerTableItem[] = [];
  protected totalRecords = 0;
  protected tableTotalRecords = 0;
  protected loading = false;
  protected globalFilter = '';
  protected activeRowAction = false;

  private draftCustomer: CustomerTableItem | null = null;
  private viewInitialized = false;

  constructor() {
    super();
    this.l10nService.get('customer-module').subscribe((l10n) => {
      this.appTitleService.setTitle(l10n.title);
    });
  }

  ngAfterViewInit(): void {
    this.viewInitialized = true;
    if (this.id !== undefined) {
      void this.loadCustomers(this.createLazyEventFromState());
    }
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    this.globalFilter = this.localSettings().globalFilter ?? '';
    this.cancelAllEdits();

    if (this.viewInitialized) {
      await this.loadCustomers(this.createLazyEventFromState());
    }
  }

  protected statusSeverity(status?: DemoCustomerStatus | null): 'success' | 'warn' | 'danger' | 'contrast' {
    switch (status) {
      case DemoCustomerStatus.Active:
        return 'success';
      case DemoCustomerStatus.Suspended:
        return 'danger';
      default:
        return 'warn';
    }
  }

  protected async loadCustomers(event: TableLazyLoadEvent): Promise<void> {
    this.loading = true;

    const filters = (event.filters ?? this.localSettings().filters ?? {}) as {
      [field: string]: FilterMetadata | FilterMetadata[];
    };

    const request: TableQueryRequest = {
      first: event.first ?? this.localSettings().first ?? 0,
      rows: event.rows ?? this.localSettings().rows ?? 10,
      sortField: typeof event.sortField === 'string' ? event.sortField : this.localSettings().sortField ?? undefined,
      sortOrder: event.sortOrder ?? this.localSettings().sortOrder ?? 1,
      globalFilter: this.globalFilter,
      filters: filters as never
    };

    try {
      const response = await this.dataService.getPage(request);
      this.customers = (response.data ?? []).map((customer) => ({ ...customer }));
      this.totalRecords = response.total ?? 0;
      this.syncVisibleCustomers();

      this.localSettings.set({
        first: request.first ?? 0,
        rows: request.rows ?? 10,
        sortField: request.sortField ?? null,
        sortOrder: request.sortOrder ?? 1,
        globalFilter: this.globalFilter,
        filters
      });
    } catch (error) {
      this.customers = [];
      this.totalRecords = 0;
      this.syncVisibleCustomers();
      this.msgService.errorFromException(error, 'Load customer error');
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

  protected beginCreateCustomer(): void {
    this.cancelAllEdits();

    this.draftCustomer = {
      fullName: '',
      email: '',
      category: '',
      country: '',
      status: DemoCustomerStatus.Prospect,
      creditScore: 0,
      lifetimeValue: 0,
      createdAt: new Date(),
      ordersCount: 0,
      _isNew: true,
      _isEditing: true,
      _editModel: this.createEditModel()
    };

    this.syncVisibleCustomers();
  }

  protected editCustomer(customer: CustomerTableItem): void {
    this.cancelAllEdits(customer);
    customer._isEditing = true;
    customer._editModel = this.createEditModel(customer);
    this.syncVisibleCustomers();
  }

  protected cancelEdit(customer: CustomerTableItem): void {
    if (customer._isNew) {
      this.draftCustomer = null;
    } else {
      customer._isEditing = false;
      customer._editModel = undefined;
    }

    this.syncVisibleCustomers();
  }

  protected async saveCustomer(customer: CustomerTableItem): Promise<void> {
    const editModel = customer._editModel;
    if (!editModel) {
      return;
    }

    const request = this.normalizeEditModel(editModel);
    if (!request.fullName || !request.email || !request.category || !request.country) {
      this.msgService.error(this.l10nService.instant('customer-module.messages.requiredFields'));
      return;
    }

    this.activeRowAction = true;

    try {
      if (customer._isNew) {
        await this.dataService.request<DemoCustomerTableRowDto>({
          path: '/api/customer-module/create',
          method: 'POST',
          body: request,
          secure: true,
          type: ContentType.Json,
          format: 'json'
        });

        this.draftCustomer = null;
        this.msgService.success(this.l10nService.instant('customer-module.messages.createSuccess'));
      } else {
        await this.dataService.request<DemoCustomerTableRowDto>({
          path: `/api/customer-module/update/${customer.id}`,
          method: 'PUT',
          body: request,
          secure: true,
          type: ContentType.Json,
          format: 'json'
        });

        customer._isEditing = false;
        customer._editModel = undefined;
        this.msgService.success(this.l10nService.instant('customer-module.messages.updateSuccess'));
      }

      await this.loadCustomers(this.createLazyEventFromState());
    } catch (error) {
      this.msgService.errorFromException(error, 'Save customer error');

    } finally {
      this.activeRowAction = false;
    }
  }

  protected async deleteCustomer(customer: CustomerTableItem): Promise<void> {
    if (customer._isNew) {
      this.cancelEdit(customer);
      return;
    }

    if (!customer.id || !window.confirm(this.l10nService.instant('customer-module.messages.deleteConfirm'))) {
      return;
    }

    this.activeRowAction = true;

    try {
      await this.dataService.request<void>({
        path: `/api/customer-module/delete/${customer.id}`,
        method: 'DELETE',
        secure: true
      });

      this.msgService.success(this.l10nService.instant('customer-module.messages.deleteSuccess'));
      await this.loadCustomers(this.createLazyEventFromState());
    } catch (error) {
      this.msgService.errorFromException(error, 'Delete customer error');

    } finally {
      this.activeRowAction = false;
    }
  }

  private createLazyEventFromState(): TableLazyLoadEvent {
    return {
      first: this.localSettings().first ?? 0,
      rows: this.localSettings().rows ?? 10,
      sortField: this.localSettings().sortField ?? undefined,
      sortOrder: this.localSettings().sortOrder ?? 1,
      filters: this.localSettings().filters ?? {}
    };
  }

  private createEditModel(customer?: DemoCustomerTableRowDto): CustomerEditModel {
    return {
      fullName: customer?.fullName ?? '',
      email: customer?.email ?? '',
      category: customer?.category ?? '',
      country: customer?.country ?? '',
      status: customer?.status ?? DemoCustomerStatus.Prospect,
      creditScore: customer?.creditScore ?? 0,
      lifetimeValue: customer?.lifetimeValue ?? 0
    };
  }

  private normalizeEditModel(model: CustomerEditModel): CustomerEditModel {
    return {
      fullName: model.fullName.trim(),
      email: model.email.trim(),
      category: model.category.trim(),
      country: model.country.trim(),
      status: model.status,
      creditScore: Number(model.creditScore) || 0,
      lifetimeValue: Number(model.lifetimeValue) || 0
    };
  }

  private cancelAllEdits(exceptCustomer?: CustomerTableItem): void {
    if (!exceptCustomer?._isNew) {
      this.draftCustomer = null;
    }

    for (const customer of this.customers) {
      if (customer === exceptCustomer) {
        continue;
      }

      customer._isEditing = false;
      customer._editModel = undefined;
    }
  }

  private syncVisibleCustomers(): void {
    this.visibleCustomers = this.draftCustomer
      ? [this.draftCustomer, ...this.customers]
      : [...this.customers];
    this.tableTotalRecords = this.totalRecords + (this.draftCustomer ? 1 : 0);
  }
}
