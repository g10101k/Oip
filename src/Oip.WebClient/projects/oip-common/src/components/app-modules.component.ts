import { Component, inject, OnInit } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { MsgService } from '../services/msg.service';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { InputTextModule } from 'primeng/inputtext';
import { L10nService } from '../services/l10n.service';
import { ExtensionModulesApi } from '../api/extension-modules.api';
import { ModuleApi } from '../api/module.api';
import { ExistModuleDto } from '../api/data-contracts';
import { AppTitleService } from '../services/app-title.service';
import { TranslatePipe } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

@Component({
  imports: [FormsModule, TableModule, Tag, ButtonModule, ToolbarModule, Tooltip, ConfirmDialog, TranslatePipe, InputTextModule],
  providers: [ConfirmationService, ModuleApi, ExtensionModulesApi],
  selector: 'app-modules',
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="flex flex-col md:flex-row gap-4">
      <div class="card w-full">
        <div class="font-semibold text-xl mb-4">
          {{ 'app-modules.title' | translate }}
        </div>
        <div class="mb-4">
          <p-toolbar>
            <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
              <div class="flex flex-col sm:flex-row gap-2 flex-1">
                <input
                  class="w-full"
                  pInputText
                  type="url"
                  [disabled]="registeringExternalModule"
                  [placeholder]="'app-modules.register.manifestUrlPlaceholder' | translate"
                  [(ngModel)]="externalModuleManifestUrl"
                  (keydown.enter)="registerExternalModule()" />
                <p-button
                  icon="pi pi-plus"
                  severity="success"
                  [disabled]="!canRegisterExternalModule"
                  [label]="'app-modules.register.button' | translate"
                  [loading]="registeringExternalModule"
                  (onClick)="registerExternalModule()"></p-button>
              </div>
              <p-button
                icon="pi pi-refresh"
                rounded="true"
                severity="secondary"
                text="true"
                tooltipPosition="bottom"
                [pTooltip]="'app-modules.refreshTooltip' | translate"
                (onClick)="refreshAction()"></p-button>
            </div>
          </p-toolbar>
        </div>
        <p-table class="mt-4" [paginator]="true" [rows]="100" [value]="modules">
          <ng-template pTemplate="header">
            <tr>
              <th>{{ 'app-modules.table.moduleId' | translate }}</th>
              <th>{{ 'app-modules.table.name' | translate }}</th>
              <th>{{ 'app-modules.table.currentlyLoaded' | translate }}</th>
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
        </p-table>
      </div>
    </div>
  `
})
export class AppModulesComponent implements OnInit {
  protected modules: ExistModuleDto[] = [];
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

  async refreshAction() {
    this.modules = await this.moduleService.getModulesWithLoadStatus();
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
