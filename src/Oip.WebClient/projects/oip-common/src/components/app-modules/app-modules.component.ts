import { Component, inject, OnInit } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { MsgService } from '../../services/msg.service';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { L10nService } from '../../services/l10n.service';
import { ExtensionModulesApi } from '../../api/extension-modules.api';
import { ModuleApi } from '../../api/module.api';
import { ExistModuleDto } from '../../api/data-contracts';
import { AppTitleService } from '../../services/app-title.service';
import { TranslatePipe } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import en from './l10n/app-modules.en.json';
import ru from './l10n/app-modules.ru.json';

L10nService.registerTranslations({ en, ru });

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
    InputTextModule
  ],
  providers: [ConfirmationService, ModuleApi, ExtensionModulesApi],
  selector: 'app-modules',
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card w-full">
        <div class="mb-4">
          <p-toolbar>
            <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
              <div class="font-semibold text-lg flex items-center gap-2 mx-2">
                <i class="pi pi-box"></i>
                {{ 'app-modules.title' | translate }}
              </div>

              <div class="flex items-center gap-1 flex-1">
                <input
                  class="w-full"
                  pInputText
                  type="url"
                  id="oip-app-modules-manifest-url"
                  [disabled]="registeringExternalModule"
                  [placeholder]="'app-modules.register.manifestUrlPlaceholder' | translate"
                  [(ngModel)]="externalModuleManifestUrl"
                  (keydown.enter)="registerExternalModule()" />
                <p-button
                  icon="pi pi-plus"
                  rounded="true"
                  severity="success"
                  text="true"
                  tooltipPosition="bottom"
                  id="oip-app-modules-register"
                  [disabled]="!canRegisterExternalModule"
                  [pTooltip]="'app-modules.register.button' | translate"
                  [loading]="registeringExternalModule"
                  (onClick)="registerExternalModule()"></p-button>
              </div>

              <div class="flex items-center gap-1 w-full md:w-auto">
                <p-button
                  icon="pi pi-refresh"
                  rounded="true"
                  severity="secondary"
                  text="true"
                  tooltipPosition="bottom"
                  [loading]="loading"
                  [pTooltip]="'app-modules.refreshTooltip' | translate"
                  (onClick)="refreshAction()"></p-button>
                <input
                  class="w-full md:w-96"
                  pInputText
                  type="text"
                  id="oip-app-modules-filter"
                  [placeholder]="'app-modules.filterPlaceholder' | translate"
                  [(ngModel)]="moduleFilter" />
                <p-button
                  icon="pi pi-filter-slash"
                  rounded="true"
                  severity="secondary"
                  text="true"
                  tooltipPosition="bottom"
                  id="oip-app-modules-clear-filter"
                  [disabled]="!moduleFilter"
                  [pTooltip]="'app-modules.clearFilterTooltip' | translate"
                  (onClick)="moduleFilter = ''"></p-button>
              </div>
            </div>
          </p-toolbar>
        </div>
        <p-table
          class="mt-4"
          dataKey="moduleId"
          sortField="name"
          [sortOrder]="1"
          [paginator]="true"
          [rows]="100"
          [value]="filteredModules"
          [loading]="loading">
          <ng-template pTemplate="header">
            <tr>
              <th pSortableColumn="moduleId">
                {{ 'app-modules.table.moduleId' | translate }}
                <p-sortIcon field="moduleId"></p-sortIcon>
              </th>
              <th pSortableColumn="name">
                {{ 'app-modules.table.name' | translate }}
                <p-sortIcon field="name"></p-sortIcon>
              </th>
              <th pSortableColumn="currentlyLoaded">
                {{ 'app-modules.table.currentlyLoaded' | translate }}
                <p-sortIcon field="currentlyLoaded"></p-sortIcon>
              </th>
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
          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="4">{{ 'app-modules.table.empty' | translate }}</td>
            </tr>
          </ng-template>
        </p-table>
      </div>
    </div>
  `
})
export class AppModulesComponent implements OnInit {
  protected modules: ExistModuleDto[] = [];
  protected moduleFilter = '';
  protected loading = false;
  protected msgService = inject(MsgService);
  protected confirmationService = inject(ConfirmationService);
  protected l10nService = inject(L10nService);
  protected titleService = inject(AppTitleService);
  protected externalModuleManifestUrl = '';
  protected registeringExternalModule = false;
  private moduleService = inject(ModuleApi);
  private extensionModulesService = inject(ExtensionModulesApi);
  private translationsReady: Promise<unknown>;

  constructor() {
    this.translationsReady = firstValueFrom(this.l10nService.loadComponentTranslations('app-modules'));
  }

  async ngOnInit() {
    await this.translationsReady;
    this.titleService.setTitle(this.t('app-modules.title'));
    await this.refreshAction();
  }

  protected get filteredModules(): ExistModuleDto[] {
    const filter = this.moduleFilter.trim().toLowerCase();
    if (!filter) {
      return this.modules;
    }
    return this.modules.filter((module) =>
      [module.moduleId, module.name].some((value) => value?.toString().toLowerCase().includes(filter))
    );
  }

  async refreshAction() {
    this.loading = true;
    try {
      this.modules = await this.moduleService.getModulesWithLoadStatus();
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.t('app-modules.messages.loadError'),
        this.t('app-modules.messages.loadError')
      );
    } finally {
      this.loading = false;
    }
  }

  protected get canRegisterExternalModule() {
    return !!this.externalModuleManifestUrl.trim() && !this.registeringExternalModule;
  }

  async registerExternalModule() {
    if (!this.canRegisterExternalModule) {
      return;
    }

    await this.translationsReady;
    this.registeringExternalModule = true;

    try {
      await this.extensionModulesService.registerExtensionModule({
        manifestUrl: this.externalModuleManifestUrl.trim()
      });
      this.externalModuleManifestUrl = '';
      await this.refreshAction();
      this.msgService.success(this.t('app-modules.messages.registerSuccess'));
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.t('app-modules.messages.registerError'),
        this.t('app-modules.messages.registerError')
      );
    } finally {
      this.registeringExternalModule = false;
    }
  }

  async deleteModule(module: ExistModuleDto) {
    await this.translationsReady;

    this.confirmationService.confirm({
      header: this.t('app-modules.confirm.header'),
      message: this.t('app-modules.confirm.message'),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.t('app-modules.confirm.cancel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.t('app-modules.confirm.delete'),
        severity: 'danger'
      },
      accept: async () => {
        try {
          await this.moduleService.delete({
            moduleId: module.moduleId
          });
          await this.refreshAction();
          this.msgService.success(this.t('app-modules.messages.deleteSuccess'));
        } catch (error) {
          this.msgService.errorFromException(
            error,
            this.t('app-modules.messages.deleteError'),
            this.t('app-modules.messages.deleteError')
          );
        }
      }
    });
  }

  t(key: string) {
    return this.l10nService.instant(key);
  }
}
