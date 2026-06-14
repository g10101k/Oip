import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { InputText } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ToolbarModule } from 'primeng/toolbar';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { ApplicationsApi } from '../api/applications.api';
import { ApplicationRegistryItemDto, ServiceType } from '../api/applications-data-contracts';
import { AppTitleService } from '../services/app-title.service';
import { L10nService } from '../services/l10n.service';
import { MsgService } from '../services/msg.service';

interface ApplicationEditModel {
  code: string;
  displayName: string;
  baseUrl: string;
  internalBaseUrl: string;
  icon: string;
  order: number;
  enabled: boolean;
  serviceType: NonNullable<ApplicationRegistryItemDto['serviceType']>;
}

interface ApplicationTableItem extends ApplicationRegistryItemDto {
  _isEditing?: boolean;
  _isNew?: boolean;
  _editModel?: ApplicationEditModel;
}

@Component({
  imports: [
    FormsModule,
    TableModule,
    Tag,
    ButtonModule,
    ToolbarModule,
    Tooltip,
    ConfirmDialog,
    TranslatePipe,
    InputText,
    SelectModule,
    ToggleSwitchModule
  ],
  providers: [ApplicationsApi, ConfirmationService],
  selector: 'app-applications',
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="flex flex-col gap-4">
      <div class="card w-full">
        <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <div class="font-semibold text-xl mb-1">
              {{ 'applications.title' | translate }}
            </div>
            <p class="m-0 text-surface-500">{{ 'applications.subtitle' | translate }}</p>
          </div>
          <div class="flex flex-col gap-2 sm:flex-row">
            <input
              class="w-full sm:w-72"
              pInputText
              type="text"
              [placeholder]="'applications.searchPlaceholder' | translate"
              [(ngModel)]="globalFilter"
              (keydown.enter)="applyGlobalFilter()" />
            <p-button
              icon="pi pi-search"
              [label]="'applications.search' | translate"
              (onClick)="applyGlobalFilter()"></p-button>
            <p-button
              icon="pi pi-filter-slash"
              severity="secondary"
              [label]="'applications.clear' | translate"
              (onClick)="clearFilters()"></p-button>
          </div>
        </div>

        <div class="my-4">
          <p-toolbar>
            <p-button
              icon="pi pi-plus"
              severity="success"
              [disabled]="loading || activeRowAction"
              [label]="'applications.add' | translate"
              (onClick)="beginCreateApplication()"></p-button>
            <p-button
              icon="pi pi-refresh"
              rounded="true"
              severity="secondary"
              text="true"
              tooltipPosition="bottom"
              [disabled]="loading || activeRowAction"
              [pTooltip]="'applications.refreshTooltip' | translate"
              (onClick)="refreshAction()"></p-button>
          </p-toolbar>
        </div>

        <p-table
          #table
          dataKey="code"
          responsiveLayout="scroll"
          [globalFilterFields]="globalFilterFields"
          [loading]="loading"
          [paginator]="true"
          [rows]="25"
          [rowsPerPageOptions]="[10, 25, 50, 100]"
          [value]="visibleApplications">
          <ng-template pTemplate="header">
            <tr>
              <th pSortableColumn="code">
                {{ 'applications.table.code' | translate }}
                <p-sortIcon field="code"></p-sortIcon>
              </th>
              <th pSortableColumn="displayName">
                {{ 'applications.table.displayName' | translate }}
                <p-sortIcon field="displayName"></p-sortIcon>
              </th>
              <th pSortableColumn="baseUrl">
                {{ 'applications.table.baseUrl' | translate }}
                <p-sortIcon field="baseUrl"></p-sortIcon>
              </th>
              <th pSortableColumn="internalBaseUrl">
                {{ 'applications.table.internalBaseUrl' | translate }}
                <p-sortIcon field="internalBaseUrl"></p-sortIcon>
              </th>
              <th pSortableColumn="icon">
                {{ 'applications.table.icon' | translate }}
                <p-sortIcon field="icon"></p-sortIcon>
              </th>
              <th pSortableColumn="order" class="text-right">
                {{ 'applications.table.order' | translate }}
                <p-sortIcon field="order"></p-sortIcon>
              </th>
              <th class="text-center">{{ 'applications.table.enabled' | translate }}</th>
              <th pSortableColumn="serviceType">
                {{ 'applications.table.serviceType' | translate }}
                <p-sortIcon field="serviceType"></p-sortIcon>
              </th>
              <th class="text-center">{{ 'applications.table.current' | translate }}</th>
              <th class="text-center min-w-36">{{ 'applications.table.actions' | translate }}</th>
            </tr>
          </ng-template>

          <ng-template let-application pTemplate="body">
            <tr>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <input
                    class="w-full min-w-32"
                    pInputText
                    [disabled]="!application._isNew"
                    [(ngModel)]="editModel.code" />
                } @else {
                  <span class="font-medium">{{ application.code }}</span>
                }
              </td>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <input class="w-full min-w-40" pInputText [(ngModel)]="editModel.displayName" />
                } @else {
                  {{ application.displayName }}
                }
              </td>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <input class="w-full min-w-56" pInputText [(ngModel)]="editModel.baseUrl" />
                } @else {
                  <span class="break-all">{{ application.baseUrl }}</span>
                }
              </td>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <input class="w-full min-w-56" pInputText [(ngModel)]="editModel.internalBaseUrl" />
                } @else {
                  <span class="break-all">{{ application.internalBaseUrl }}</span>
                }
              </td>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <input class="w-full min-w-32" pInputText [(ngModel)]="editModel.icon" />
                } @else {
                  <div class="flex items-center gap-2">
                    <i [class]="application.icon || 'pi pi-th-large'"></i>
                    <span>{{ application.icon }}</span>
                  </div>
                }
              </td>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <input class="w-full min-w-24 text-right" pInputText type="number" [(ngModel)]="editModel.order" />
                } @else {
                  <span class="block text-right tabular-nums">{{ application.order }}</span>
                }
              </td>
              <td class="text-center">
                @if (application._isEditing && application._editModel; as editModel) {
                  <p-toggle-switch [(ngModel)]="editModel.enabled"></p-toggle-switch>
                } @else {
                  <p-tag
                    [severity]="application.enabled ? 'success' : 'danger'"
                    [value]="
                      (application.enabled ? 'applications.table.yes' : 'applications.table.no') | translate
                  "></p-tag>
                }
              </td>
              <td>
                @if (application._isEditing && application._editModel; as editModel) {
                  <p-select
                    class="w-full min-w-36"
                    optionLabel="label"
                    optionValue="value"
                    appendTo="body"
                    [options]="serviceTypeOptions"
                    [(ngModel)]="editModel.serviceType"></p-select>
                } @else {
                  {{ serviceTypeLabel(application.serviceType) }}
                }
              </td>
              <td class="text-center">
                <p-tag
                  [severity]="application.isCurrent ? 'info' : 'secondary'"
                  [value]="
                    (application.isCurrent ? 'applications.table.yes' : 'applications.table.no') | translate
                  "></p-tag>
              </td>
              <td>
                <div class="flex items-center justify-center gap-2">
                  @if (application._isEditing) {
                    <p-button
                      icon="pi pi-check"
                      severity="success"
                      text="true"
                      [disabled]="activeRowAction"
                      [pTooltip]="'applications.table.saveTooltip' | translate"
                      tooltipPosition="bottom"
                      (onClick)="saveApplication(application)"></p-button>
                    <p-button
                      icon="pi pi-times"
                      severity="secondary"
                      text="true"
                      [disabled]="activeRowAction"
                      [pTooltip]="'applications.table.cancelTooltip' | translate"
                      tooltipPosition="bottom"
                      (onClick)="cancelEdit(application)"></p-button>
                  } @else {
                    <p-button
                      icon="pi pi-pencil"
                      text="true"
                      [disabled]="activeRowAction"
                      [pTooltip]="'applications.table.editTooltip' | translate"
                      tooltipPosition="bottom"
                      (onClick)="editApplication(application)"></p-button>
                    <p-button
                      icon="pi pi-trash"
                      severity="danger"
                      text="true"
                      [disabled]="activeRowAction"
                      [pTooltip]="'applications.table.deleteTooltip' | translate"
                      tooltipPosition="bottom"
                      (onClick)="deleteApplication(application)"></p-button>
                  }
                </div>
              </td>
            </tr>
          </ng-template>

          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="10" class="py-8 text-center text-surface-500">
                {{ 'applications.empty' | translate }}
              </td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    </div>
  `
})
export class ApplicationsComponent implements OnInit {
  protected readonly globalFilterFields = ['code', 'displayName', 'baseUrl', 'internalBaseUrl', 'icon', 'serviceType'];
  protected get serviceTypeOptions(): {
    label: string;
    value: NonNullable<ApplicationRegistryItemDto['serviceType']>;
  }[] {
    return [
      {
        label: this.t('applications.serviceTypes.service'),
        value: ServiceType.Service
      },
      {
        label: this.t('applications.serviceTypes.application'),
        value: ServiceType.Application
      }
    ];
  }
  protected applications: ApplicationTableItem[] = [];
  protected visibleApplications: ApplicationTableItem[] = [];
  protected globalFilter = '';
  protected loading = false;
  protected activeRowAction = false;

  private readonly applicationsApi = inject(ApplicationsApi);
  private readonly msgService = inject(MsgService);
  private readonly l10nService = inject(L10nService);
  private readonly titleService = inject(AppTitleService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly translationsReady: Promise<unknown>;
  private draftApplication: ApplicationTableItem | null = null;

  constructor() {
    this.translationsReady = firstValueFrom(this.l10nService.loadComponentTranslations('applications'));
  }

  async ngOnInit(): Promise<void> {
    await this.translationsReady;
    this.titleService.setTitle(this.t('applications.title'));
    await this.refreshAction();
  }

  protected async refreshAction(): Promise<void> {
    this.loading = true;
    this.cancelAllEdits();

    try {
      this.applications = (await this.applicationsApi.getApplicationRegistryItems()).map(
        (application: ApplicationRegistryItemDto) => ({ ...application })
      );
      this.syncVisibleApplications();
    } catch (error) {
      this.applications = [];
      this.syncVisibleApplications();
      this.msgService.error(error);
    } finally {
      this.loading = false;
    }
  }

  protected applyGlobalFilter(): void {
    this.visibleApplications = this.filterApplications(this.globalFilter);
  }

  protected clearFilters(): void {
    this.globalFilter = '';
    this.syncVisibleApplications();
  }

  protected beginCreateApplication(): void {
    this.cancelAllEdits();
    this.draftApplication = {
      code: '',
      displayName: '',
      baseUrl: '',
      internalBaseUrl: '',
      icon: 'pi pi-th-large',
      order: this.getNextOrder(),
      enabled: true,
      serviceType: ServiceType.Service,
      isCurrent: false,
      _isNew: true,
      _isEditing: true,
      _editModel: this.createEditModel()
    };
    this.draftApplication._editModel.order = this.draftApplication.order ?? 0;
    this.syncVisibleApplications();
  }

  protected editApplication(application: ApplicationTableItem): void {
    this.cancelAllEdits(application);
    application._isEditing = true;
    application._editModel = this.createEditModel(application);
    this.syncVisibleApplications();
  }

  protected cancelEdit(application: ApplicationTableItem): void {
    if (application._isNew) {
      this.draftApplication = null;
    } else {
      application._isEditing = false;
      application._editModel = undefined;
    }

    this.syncVisibleApplications();
  }

  protected async saveApplication(application: ApplicationTableItem): Promise<void> {
    const editModel = application._editModel;
    if (!editModel) {
      return;
    }

    const request = this.normalizeEditModel(editModel);
    if (!request.code || !request.displayName || !request.baseUrl) {
      this.msgService.error(this.t('applications.messages.requiredFields'));
      return;
    }

    this.activeRowAction = true;

    try {
      if (application._isNew) {
        await this.applicationsApi.createApplicationRegistryItem(request);
        this.draftApplication = null;
        this.msgService.success(this.t('applications.messages.createSuccess'));
      } else {
        await this.applicationsApi.updateApplicationRegistryItem({ code: application.code as string }, request);
        application._isEditing = false;
        application._editModel = undefined;
        this.msgService.success(this.t('applications.messages.updateSuccess'));
      }

      await this.refreshAction();
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.activeRowAction = false;
    }
  }

  protected async deleteApplication(application: ApplicationTableItem): Promise<void> {
    await this.translationsReady;

    if (application._isNew) {
      this.cancelEdit(application);
      return;
    }

    if (!application.code) {
      return;
    }

    this.confirmationService.confirm({
      header: this.t('applications.confirm.header'),
      message: this.t('applications.confirm.message', { displayName: application.displayName || application.code }),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.t('applications.confirm.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.t('applications.confirm.delete'),
        severity: 'danger'
      },
      accept: async () => {
        this.activeRowAction = true;

        try {
          await this.applicationsApi.deleteApplicationRegistryItem({ code: application.code as string });
          this.msgService.success(this.t('applications.messages.deleteSuccess'));
          await this.refreshAction();
        } catch (error) {
          this.msgService.error(error);
        } finally {
          this.activeRowAction = false;
        }
      }
    });
  }

  private createEditModel(application?: ApplicationRegistryItemDto): ApplicationEditModel {
    return {
      code: application?.code ?? '',
      displayName: application?.displayName ?? '',
      baseUrl: application?.baseUrl ?? '',
      internalBaseUrl: application?.internalBaseUrl ?? '',
      icon: application?.icon ?? 'pi pi-th-large',
      order: application?.order ?? 0,
      enabled: application?.enabled ?? true,
      serviceType: application?.serviceType ?? ServiceType.Service
    };
  }

  private normalizeEditModel(model: ApplicationEditModel): ApplicationRegistryItemDto {
    return {
      code: model.code.trim(),
      displayName: model.displayName.trim(),
      baseUrl: model.baseUrl.trim(),
      internalBaseUrl: model.internalBaseUrl.trim(),
      icon: model.icon.trim(),
      order: Number(model.order) || 0,
      enabled: model.enabled,
      serviceType: model.serviceType
    };
  }

  protected serviceTypeLabel(serviceType: ApplicationRegistryItemDto['serviceType']): string {
    return this.t(
      serviceType === ServiceType.Application
        ? 'applications.serviceTypes.application'
        : 'applications.serviceTypes.service'
    );
  }

  private cancelAllEdits(exceptApplication?: ApplicationTableItem): void {
    if (!exceptApplication?._isNew) {
      this.draftApplication = null;
    }

    for (const application of this.applications) {
      if (application === exceptApplication) {
        continue;
      }

      application._isEditing = false;
      application._editModel = undefined;
    }
  }

  private syncVisibleApplications(): void {
    this.visibleApplications = this.filterApplications(this.globalFilter);
  }

  private filterApplications(filter: string): ApplicationTableItem[] {
    const source = this.draftApplication
      ? [this.draftApplication, ...this.applications]
      : [...this.applications];
    const normalizedFilter = filter.trim().toLocaleLowerCase();

    if (!normalizedFilter) {
      return source;
    }

    return source.filter((application) =>
      this.globalFilterFields.some((field) =>
        String(application[field as keyof ApplicationRegistryItemDto] ?? '')
          .toLocaleLowerCase()
          .includes(normalizedFilter)
      )
    );
  }

  private getNextOrder(): number {
    return Math.max(0, ...this.applications.map((application) => application.order ?? 0)) + 1;
  }

  private t(key: string, params?: Record<string, unknown>): string {
    return this.l10nService.instant(key, params) as string;
  }
}
