import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { ToolbarModule } from 'primeng/toolbar';
import { Tooltip } from 'primeng/tooltip';
import { BaseModuleComponent, NoSettingsDto, provideTranslations, SecurityComponent } from 'oip-common';
import { LlmProviderModuleApi } from '../../../api/llm-provider-module.api';
import {
  LlmProviderDto,
  LlmProviderModuleSettings,
  LlmProviderType,
  SaveLlmProviderRequest,
  TestLlmProviderResponse
} from '../../../api/data-contracts';
import en from './l10n/llm-provider-module.en.json';
import ru from './l10n/llm-provider-module.ru.json';

interface ProviderEditModel extends Required<Omit<SaveLlmProviderRequest, 'apiKey'>> {
  name: string;
  baseUrl: string;
  model: string;
  apiKey: string;
}

interface SelectOption<TValue = string> {
  label: string;
  value: TValue;
}

const DEFAULT_BASE_URL = 'https://api.openai.com/v1';
const DEFAULT_MODEL = 'gpt-4o-mini';

@Component({
  selector: 'app-llm-provider-module',
  providers: [LlmProviderModuleApi, ConfirmationService],
  imports: [
    SecurityComponent,
    TranslatePipe,
    Button,
    ConfirmDialog,
    InputText,
    Password,
    Select,
    ToggleSwitch,
    ToolbarModule,
    Tooltip,
    Dialog,
    TableModule,
    TagModule,
    FormsModule,
    DatePipe
  ],
  template: `
    @if (isContent) {
      <p-confirmDialog />
      <div class="flex flex-col md:flex-row gap-4">
        <div class="card w-full">
          <div class="mb-4">
            <p-toolbar>
              <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
                <div class="font-semibold text-lg flex items-center gap-2 mx-2">
                  <i class="pi pi-microchip-ai"></i>
                  {{ title }}
                </div>

                <div class="flex-1"></div>
                <div class="flex items-center gap-1 w-full md:w-auto">
                  <p-button
                    icon="pi pi-refresh"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="bottom"
                    id="oip-llm-provider-module-refresh"
                    [loading]="loading"
                    [disabled]="activeRowAction"
                    [pTooltip]="'llm-provider-module.content.refreshTooltip' | translate"
                    (onClick)="loadProviders()"></p-button>
                  <p-button
                    icon="pi pi-plus"
                    rounded="true"
                    severity="success"
                    text="true"
                    tooltipPosition="bottom"
                    id="oip-llm-provider-module-add"
                    [disabled]="loading || activeRowAction || !canEdit"
                    [pTooltip]="'llm-provider-module.content.add' | translate"
                    (onClick)="beginCreate()"></p-button>
                  <input
                    class="w-full md:w-96"
                    pInputText
                    type="text"
                    id="oip-llm-provider-module-filter"
                    [placeholder]="'llm-provider-module.content.searchPlaceholder' | translate"
                    [(ngModel)]="globalFilter" />
                  <p-button
                    icon="pi pi-filter-slash"
                    rounded="true"
                    severity="secondary"
                    text="true"
                    tooltipPosition="bottom"
                    id="oip-llm-provider-module-clear-filter"
                    [disabled]="!globalFilter"
                    [pTooltip]="'llm-provider-module.content.clear' | translate"
                    (onClick)="globalFilter = ''"></p-button>
                </div>
              </div>
            </p-toolbar>
          </div>

          <p-table
            class="mt-4"
            dataKey="id"
            [paginator]="true"
            [rows]="50"
            [loading]="loading"
            [value]="visibleProviders">
            <ng-template pTemplate="header">
              <tr>
                <th pSortableColumn="name">
                  {{ 'llm-provider-module.content.table.name' | translate }}
                  <p-sortIcon field="name" />
                </th>
                <th pSortableColumn="providerType">
                  {{ 'llm-provider-module.content.table.providerType' | translate }}
                  <p-sortIcon field="providerType" />
                </th>
                <th pSortableColumn="baseUrl">
                  {{ 'llm-provider-module.content.table.baseUrl' | translate }}
                  <p-sortIcon field="baseUrl" />
                </th>
                <th pSortableColumn="model">
                  {{ 'llm-provider-module.content.table.model' | translate }}
                  <p-sortIcon field="model" />
                </th>
                <th>{{ 'llm-provider-module.content.table.apiKey' | translate }}</th>
                <th class="text-center">{{ 'llm-provider-module.content.table.status' | translate }}</th>
                <th pSortableColumn="updatedAt">
                  {{ 'llm-provider-module.content.table.updatedAt' | translate }}
                  <p-sortIcon field="updatedAt" />
                </th>
                <th class="text-center min-w-44">{{ 'llm-provider-module.content.table.actions' | translate }}</th>
              </tr>
            </ng-template>

            <ng-template let-provider pTemplate="body">
              <tr>
                <td>
                  <div class="flex items-center gap-2">
                    <span class="font-medium">{{ provider.name }}</span>
                    @if (provider.isDefault) {
                      <p-tag severity="info" [value]="'llm-provider-module.content.default' | translate" />
                    }
                  </div>
                </td>
                <td>{{ provider.providerType }}</td>
                <td>
                  <span class="break-all">{{ provider.baseUrl }}</span>
                </td>
                <td>{{ provider.model }}</td>
                <td>
                  @if (provider.hasApiKey) {
                    <span class="tabular-nums">{{ provider.apiKeyHint }}</span>
                  } @else {
                    <span class="text-surface-500">{{ 'llm-provider-module.content.noApiKey' | translate }}</span>
                  }
                </td>
                <td class="text-center">
                  <p-tag
                    [severity]="provider.isEnabled ? 'success' : 'danger'"
                    [value]="
                      (provider.isEnabled
                        ? 'llm-provider-module.content.enabled'
                        : 'llm-provider-module.content.disabled'
                      ) | translate
                    " />
                </td>
                <td>{{ provider.updatedAt | date: layoutService.dateTimeFormat() }}</td>
                <td>
                  <div class="flex items-center justify-center gap-1">
                    <p-button
                      icon="pi pi-bolt"
                      rounded="true"
                      severity="help"
                      text="true"
                      tooltipPosition="bottom"
                      [disabled]="activeRowAction || !canRead"
                      [loading]="testingId === provider.id"
                      [pTooltip]="'llm-provider-module.content.table.testTooltip' | translate"
                      (onClick)="testProvider(provider)"></p-button>
                    <p-button
                      icon="pi pi-pencil"
                      rounded="true"
                      text="true"
                      tooltipPosition="bottom"
                      [disabled]="activeRowAction || !canEdit"
                      [pTooltip]="'llm-provider-module.content.table.editTooltip' | translate"
                      (onClick)="beginEdit(provider)"></p-button>
                    <p-button
                      icon="pi pi-trash"
                      rounded="true"
                      severity="danger"
                      text="true"
                      tooltipPosition="bottom"
                      [disabled]="activeRowAction || !canDelete"
                      [pTooltip]="'llm-provider-module.content.table.deleteTooltip' | translate"
                      (onClick)="deleteProvider(provider)"></p-button>
                  </div>
                </td>
              </tr>
            </ng-template>

            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="8">{{ 'llm-provider-module.content.empty' | translate }}</td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>

      <p-dialog
        [header]="
          (editingId ? 'llm-provider-module.dialog.editTitle' : 'llm-provider-module.dialog.createTitle') | translate
        "
        [modal]="true"
        [style]="{ width: '40rem' }"
        [(visible)]="dialogVisible">
        @if (editModel; as model) {
          <div class="flex items-center gap-4 mb-4 mt-1">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-name">
              {{ 'llm-provider-module.dialog.name' | translate }}
            </label>
            <input
              autocomplete="off"
              class="flex-auto"
              id="oip-llm-provider-module-name"
              pInputText
              [(ngModel)]="model.name" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-type">
              {{ 'llm-provider-module.dialog.providerType' | translate }}
            </label>
            <p-select
              appendTo="body"
              class="flex-auto"
              inputId="oip-llm-provider-module-type"
              optionLabel="label"
              optionValue="value"
              [options]="providerTypeOptions"
              [(ngModel)]="model.providerType" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-base-url">
              {{ 'llm-provider-module.dialog.baseUrl' | translate }}
            </label>
            <input
              autocomplete="off"
              class="flex-auto"
              id="oip-llm-provider-module-base-url"
              pInputText
              [placeholder]="defaultBaseUrl"
              [(ngModel)]="model.baseUrl" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-api-key">
              {{ 'llm-provider-module.dialog.apiKey' | translate }}
            </label>
            <p-password
              autocomplete="new-password"
              class="flex-1"
              inputId="oip-llm-provider-module-api-key"
              [feedback]="false"
              [fluid]="true"
              [toggleMask]="true"
              [placeholder]="
                (editingId ? 'llm-provider-module.dialog.apiKeyKeep' : 'llm-provider-module.dialog.apiKeyPlaceholder')
                  | translate
              "
              [(ngModel)]="model.apiKey" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-model">
              {{ 'llm-provider-module.dialog.model' | translate }}
            </label>
            <div class="flex flex-auto items-center gap-1">
              <p-select
                appendTo="body"
                class="flex-auto"
                inputId="oip-llm-provider-module-model"
                [editable]="true"
                [emptyMessage]="'llm-provider-module.dialog.modelsEmpty' | translate"
                [filter]="modelOptions.length > 10"
                [options]="modelOptions"
                [placeholder]="defaultModel"
                [(ngModel)]="model.model" />
              <p-button
                icon="pi pi-download"
                rounded="true"
                severity="secondary"
                text="true"
                tooltipPosition="bottom"
                [disabled]="!model.baseUrl || (!model.apiKey && editingId === null)"
                [loading]="loadingModels"
                [pTooltip]="
                  modelOptions.length
                    ? ('llm-provider-module.dialog.modelsLoaded' | translate: { count: modelOptions.length })
                    : ('llm-provider-module.dialog.loadModels' | translate)
                "
                (onClick)="loadModels()"></p-button>
            </div>
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-default">
              {{ 'llm-provider-module.dialog.isDefault' | translate }}
            </label>
            <p-toggle-switch inputId="oip-llm-provider-module-default" [(ngModel)]="model.isDefault" />
          </div>

          <div class="flex items-center gap-4 mb-4">
            <label class="font-semibold w-1/3" for="oip-llm-provider-module-enabled">
              {{ 'llm-provider-module.dialog.isEnabled' | translate }}
            </label>
            <p-toggle-switch inputId="oip-llm-provider-module-enabled" [(ngModel)]="model.isEnabled" />
          </div>
        }

        <div class="flex justify-end gap-2">
          <p-button
            id="oip-llm-provider-module-cancel"
            severity="secondary"
            [disabled]="saving"
            [label]="'llm-provider-module.dialog.cancel' | translate"
            (onClick)="dialogVisible = false" />
          <p-button
            id="oip-llm-provider-module-save"
            [disabled]="saving"
            [label]="'llm-provider-module.dialog.save' | translate"
            [loading]="saving"
            (onClick)="saveProvider()" />
        </div>
      </p-dialog>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'llm-provider-module.settings.title' | translate }}</div>
            <div class="grid grid-cols-12 gap-4">
              <label class="flex items-center col-span-12 mb-2 md:col-span-4 md:mb-0" for="showDisabled">
                {{ 'llm-provider-module.settings.showDisabled' | translate }}
              </label>
              <div class="col-span-12 md:col-span-8">
                <p-toggle-switch inputId="showDisabled" [(ngModel)]="settings.showDisabled" />
              </div>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                [label]="'llm-provider-module.settings.save' | translate"
                (onClick)="saveSettings(settings)"></p-button>
            </div>
          </div>
        </div>
      </div>
    } @else if (isSecurity) {
      <security [controller]="controller" [id]="id" />
    }
  `
})
export class LlmProviderModuleComponent extends BaseModuleComponent<LlmProviderModuleSettings, NoSettingsDto> {
  private readonly translations = provideTranslations({ en, ru });

  protected readonly dataService = inject(LlmProviderModuleApi);
  private readonly confirmationService = inject(ConfirmationService);

  protected readonly defaultBaseUrl = DEFAULT_BASE_URL;
  protected readonly defaultModel = DEFAULT_MODEL;
  protected readonly providerTypeOptions: SelectOption<LlmProviderType>[] = Object.values(LlmProviderType).map(
    (type) => ({ label: type, value: type })
  );

  protected readonly globalFilterFields: (keyof LlmProviderDto)[] = ['name', 'providerType', 'baseUrl', 'model'];

  protected providers: LlmProviderDto[] = [];
  protected globalFilter = '';
  protected loading = false;
  protected saving = false;
  protected activeRowAction = false;
  protected testingId: number | null = null;

  protected dialogVisible = false;
  protected editingId: number | null = null;
  protected editModel: ProviderEditModel | null = null;
  protected modelOptions: string[] = [];
  protected loadingModels = false;

  constructor() {
    super();
    this.l10nService.get('llm-provider-module').subscribe((l10n) => {
      this.appTitleService.setTitle(l10n.title);
    });
  }

  protected get visibleProviders(): LlmProviderDto[] {
    const providers =
      this.settings?.showDisabled === false ? this.providers.filter((x) => x.isEnabled) : this.providers;
    const filter = this.globalFilter.trim().toLocaleLowerCase();
    if (!filter) return providers;

    return providers.filter((provider) =>
      this.globalFilterFields.some((field) =>
        String(provider[field] ?? '')
          .toLocaleLowerCase()
          .includes(filter)
      )
    );
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    await this.loadProviders();
  }

  protected async loadProviders(): Promise<void> {
    if (this.securityRightsLoaded && !this.canRead) {
      this.providers = [];
      return;
    }

    this.loading = true;
    try {
      this.providers = await this.dataService.getProviders();
    } catch (error) {
      this.providers = [];
      this.msgService.errorFromException(error, String(this.t('llm-provider-module.messages.loadError')));
    } finally {
      this.loading = false;
    }
  }

  protected beginCreate(): void {
    if (!this.canEdit) return;
    this.editingId = null;
    this.editModel = {
      name: '',
      providerType: LlmProviderType.OpenAi,
      baseUrl: DEFAULT_BASE_URL,
      model: DEFAULT_MODEL,
      apiKey: '',
      isDefault: this.providers.length === 0,
      isEnabled: true
    };
    this.modelOptions = [];
    this.dialogVisible = true;
  }

  protected beginEdit(provider: LlmProviderDto): void {
    if (!this.canEdit || provider.id === undefined) return;
    this.editingId = provider.id;
    this.editModel = {
      name: provider.name ?? '',
      providerType: provider.providerType ?? LlmProviderType.OpenAi,
      baseUrl: provider.baseUrl ?? DEFAULT_BASE_URL,
      model: provider.model ?? DEFAULT_MODEL,
      apiKey: '',
      isDefault: provider.isDefault ?? false,
      isEnabled: provider.isEnabled ?? true
    };
    this.modelOptions = [];
    this.dialogVisible = true;
    if (provider.hasApiKey) void this.loadModels(true);
  }

  protected async loadModels(silent = false): Promise<void> {
    const model = this.editModel;
    if (!model || !model.baseUrl.trim()) return;

    this.loadingModels = true;
    try {
      this.modelOptions = await this.dataService.getProviderModels({
        providerId: this.editingId,
        baseUrl: model.baseUrl.trim(),
        apiKey: model.apiKey.trim() || null
      });
      if (!this.modelOptions.length && !silent) {
        this.msgService.error(this.t('llm-provider-module.messages.modelsEmpty'));
      }
    } catch (error) {
      this.modelOptions = [];
      if (!silent) {
        this.msgService.errorFromException(error, String(this.t('llm-provider-module.messages.modelsError')));
      }
    } finally {
      this.loadingModels = false;
    }
  }

  protected async saveProvider(): Promise<void> {
    const model = this.editModel;
    if (!model || !this.canEdit) return;

    const request: SaveLlmProviderRequest = {
      name: model.name.trim(),
      providerType: model.providerType,
      baseUrl: model.baseUrl.trim() || DEFAULT_BASE_URL,
      model: model.model.trim() || DEFAULT_MODEL,
      apiKey: model.apiKey.trim() || null,
      isDefault: model.isDefault,
      isEnabled: model.isEnabled
    };

    if (!request.name) {
      this.msgService.error(this.t('llm-provider-module.messages.nameRequired'));
      return;
    }
    if (this.editingId === null && !request.apiKey) {
      this.msgService.error(this.t('llm-provider-module.messages.apiKeyRequired'));
      return;
    }

    this.saving = true;
    try {
      if (this.editingId === null) {
        await this.dataService.createProvider(request);
        this.msgService.success(this.t('llm-provider-module.messages.createSuccess'));
      } else {
        await this.dataService.updateProvider({ id: this.editingId }, request);
        this.msgService.success(this.t('llm-provider-module.messages.updateSuccess'));
      }
      this.dialogVisible = false;
      await this.loadProviders();
    } catch (error) {
      this.msgService.errorFromException(error, String(this.t('llm-provider-module.messages.saveError')));
    } finally {
      this.saving = false;
    }
  }

  protected deleteProvider(provider: LlmProviderDto): void {
    if (!this.canDelete || provider.id === undefined) return;
    const id = provider.id;

    this.confirmationService.confirm({
      header: String(this.t('llm-provider-module.confirm.header')),
      message: String(this.t('llm-provider-module.confirm.message', { name: provider.name })),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: String(this.t('llm-provider-module.confirm.cancel')),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: String(this.t('llm-provider-module.confirm.delete')),
        severity: 'danger'
      },
      accept: async () => {
        this.activeRowAction = true;
        try {
          await this.dataService.deleteProvider({ id });
          this.msgService.success(this.t('llm-provider-module.messages.deleteSuccess'));
          await this.loadProviders();
        } catch (error) {
          this.msgService.errorFromException(error, String(this.t('llm-provider-module.messages.deleteError')));
        } finally {
          this.activeRowAction = false;
        }
      }
    });
  }

  protected async testProvider(provider: LlmProviderDto): Promise<void> {
    if (!this.canRead || provider.id === undefined) return;

    this.activeRowAction = true;
    this.testingId = provider.id;
    try {
      const result: TestLlmProviderResponse = await this.dataService.testProvider({ id: provider.id });
      const details = `${result.message} (${result.elapsedMs} ms)`;
      if (result.success) {
        const modelNote = result.modelFound
          ? ''
          : ` ${this.t('llm-provider-module.messages.modelNotFound', { model: provider.model })}`;
        this.msgService.success(`${details}${modelNote}`);
      } else {
        this.msgService.error(details);
      }
    } catch (error) {
      this.msgService.errorFromException(error, String(this.t('llm-provider-module.messages.testError')));
    } finally {
      this.testingId = null;
      this.activeRowAction = false;
    }
  }
}
