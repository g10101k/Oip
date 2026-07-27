import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Tag } from 'primeng/tag';
import { RouterLink } from '@angular/router';
import { BaseModuleComponent, ContentType, SecurityComponent } from 'oip-common';

interface ReportModuleSettings {
  defaultReportId?: string | null;
}

interface ReportModuleLocalSettings {
  selectedReportId?: string | null;
}

interface ReportDefinitionSummary {
  id?: string | null;
  name?: string | null;
  description?: string | null;
  currentVersion?: number;
  dataSourceKey?: string | null;
}

interface ReportParameterDefinition {
  name?: string | null;
  label?: string | null;
  required?: boolean;
  defaultValue?: string | null;
}

interface ReportDefinition {
  id?: string | null;
  name?: string | null;
  description?: string | null;
  currentVersion?: number;
  parameters?: ReportParameterDefinition[] | null;
}

interface ReportExecutionLog {
  rowCount?: number;
  cacheKey?: string | null;
  steps?: string[] | null;
}

interface ReportResult {
  status?: string | null;
  isCached?: boolean;
  document?: {
    html?: string | null;
  } | null;
  executionLog?: ReportExecutionLog | null;
}

interface ReportExportResult {
  fileName?: string | null;
  contentBase64?: string | null;
  isCached?: boolean;
  status?: string | null;
}

interface SelectOption<TValue = string> {
  label: string;
  value: TValue;
}

@Component({
  standalone: true,
  template: `
    @if (isContent) {
      <div class="card space-y-6">
        <div class="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div class="space-y-2">
            <p class="text-xs font-semibold uppercase tracking-[0.24em] text-teal-600">{{ 'report.content.eyebrow' | translate }}</p>
            <div>
              <h4 class="mb-2">{{ title }}</h4>
              <p class="m-0 max-w-2xl text-surface-500">{{ 'report.content.subtitle' | translate }}</p>
            </div>
          </div>
          <div class="rounded-2xl bg-surface-50 px-4 py-3 text-sm text-surface-600">
            <div>{{ 'report.content.statusLabel' | translate }}:
              <span class="font-semibold text-surface-900">{{ lastStatus || ('report.content.idle' | translate) }}</span>
            </div>
            <div>{{ 'report.content.cacheLabel' | translate }}:
              <span class="font-semibold text-surface-900">{{ lastCached ? ('report.content.cached' | translate) : ('report.content.fresh' | translate) }}</span>
            </div>
          </div>
        </div>

        <div class="grid gap-6 xl:grid-cols-[360px_minmax(0,1fr)]">
          <section class="rounded-3xl border border-surface-200 bg-white p-5 shadow-sm">
            <div class="space-y-4">
              <div>
                <label class="mb-2 block text-sm font-semibold">{{ 'report.content.report' | translate }}</label>
                <p-select
                  class="w-full"
                  optionLabel="label"
                  optionValue="value"
                  [options]="reportOptions"
                  [disabled]="loading"
                  [(ngModel)]="selectedReportId"
                  (ngModelChange)="onReportChange($event)"/>
              </div>

              @for (parameter of parameters; track parameter.name) {
                <div>
                  <label class="mb-2 block text-sm font-semibold">
                    {{ parameter.label || parameter.name }}
                    @if (parameter.required) {
                      <span class="text-red-500">*</span>
                    }
                  </label>
                  <input
                    pInputText
                    class="w-full"
                    [disabled]="loading"
                    [(ngModel)]="parameterValues[parameter.name || '']"/>
                </div>
              }

              <div class="flex flex-col gap-2 pt-2">
                <a
                  class="p-button p-button-secondary justify-center"
                  [routerLink]="['/report-designer', id]"
                  [queryParams]="{reportId: selectedReportId}">
                  <i class="pi pi-pencil"></i><span>Дизайнер</span>
                </a>
                <p-button
                  icon="pi pi-eye"
                  [label]="'report.content.preview' | translate"
                  [disabled]="loading || !selectedReportId"
                  (onClick)="runPreview()"/>
                <p-button
                  icon="pi pi-download"
                  severity="secondary"
                  [label]="'report.content.export' | translate"
                  [disabled]="loading || !selectedReportId"
                  (onClick)="exportHtml()"/>
              </div>
            </div>
          </section>

          <section class="overflow-hidden rounded-[28px] border border-surface-200 bg-surface-50 shadow-sm">
            <div class="flex items-center justify-between border-b border-surface-200 bg-white px-5 py-4">
              <div>
                <div class="text-sm font-semibold text-surface-900">{{ activeDefinition?.name || ('report.content.previewPanel' | translate) }}</div>
                <div class="text-sm text-surface-500">{{ activeDefinition?.description }}</div>
              </div>
              @if (lastRowCount !== null) {
                <p-tag [value]="('report.content.rows' | translate) + ': ' + lastRowCount" severity="contrast"/>
              }
            </div>
            <div class="min-h-[640px] bg-white p-4">
              @if (loading) {
                <div class="flex h-full min-h-[560px] items-center justify-center text-surface-500">
                  {{ 'report-module.content.loading' | translate }}
                </div>
              } @else if (previewHtml) {
                <iframe
                  class="h-[640px] w-full rounded-2xl border border-surface-200"
                  sandbox="allow-same-origin"
                  [srcdoc]="previewHtml"></iframe>
              } @else {
                <div class="flex h-full min-h-[560px] items-center justify-center rounded-2xl border border-dashed border-surface-300 bg-surface-50 text-center text-surface-500">
                  {{ 'report-module.content.empty' | translate }}
                </div>
              }
            </div>
          </section>
        </div>
      </div>
    }

    @if (isSettings) {
      <div class="card">
        <div class="font-semibold text-xl">{{ 'report.settings.title' | translate }}</div>
        <p class="mt-3 mb-4 text-surface-500">{{ 'report.settings.description' | translate }}</p>
        <div class="max-w-xl">
          <label class="mb-2 block text-sm font-semibold">{{ 'report.settings.defaultReport' | translate }}</label>
          <input pInputText class="w-full" [(ngModel)]="settings.defaultReportId"/>
          <div class="mt-4">
            <p-button icon="pi pi-save" [label]="'report.settings.save' | translate" (onClick)="saveSettings(settings)"/>
          </div>
        </div>
      </div>
    }

    @if (isSecurity) {
      <security [controller]="controller" [id]="id"/>
    }
  `,
  imports: [CommonModule, FormsModule, RouterLink, SecurityComponent, TranslatePipe, Button, InputText, Select, Tag]
})
export class ReportModuleComponent extends BaseModuleComponent<ReportModuleSettings, ReportModuleLocalSettings> implements OnInit {
  protected reportOptions: SelectOption[] = [];
  protected selectedReportId = '';
  protected parameters: ReportParameterDefinition[] = [];
  protected parameterValues: Record<string, string> = {};
  protected previewHtml = '';
  protected activeDefinition: ReportDefinition | null = null;
  protected loading = false;
  protected lastStatus = '';
  protected lastCached = false;
  protected lastRowCount: number | null = null;

  constructor() {
    super();
    this.l10nService.get('report').subscribe((l10n) => {
      this.appTitleService.setTitle(l10n.title);
    });
  }

  override async ngOnInit(): Promise<void> {
    await super.ngOnInit();
    await this.loadReports();
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    this.selectedReportId = this.localSettings().selectedReportId ?? this.settings.defaultReportId ?? '';
    await this.loadReports();
  }

  protected async onReportChange(reportId: string): Promise<void> {
    this.localSettings.set({
      selectedReportId: reportId
    });
    await this.loadDefinition(reportId);
  }

  protected async runPreview(): Promise<void> {
    if (!this.selectedReportId) {
      return;
    }

    this.loading = true;
    try {
      const response = await this.httpClient.request<ReportResult>({
        path: '/api/report/get-report-preview',
        method: 'POST',
        secure: true,
        type: ContentType.Json,
        format: 'json',
        body: this.createRequestBody()
      });

      this.previewHtml = response.document?.html ?? '';
      this.lastStatus = response.status ?? '';
      this.lastCached = response.isCached ?? false;
      this.lastRowCount = response.executionLog?.rowCount ?? null;
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.loading = false;
    }
  }

  protected async exportHtml(): Promise<void> {
    if (!this.selectedReportId) {
      return;
    }

    this.loading = true;
    try {
      const response = await this.httpClient.request<ReportExportResult>({
        path: '/api/report/get-report-export',
        method: 'POST',
        secure: true,
        type: ContentType.Json,
        format: 'json',
        body: this.createRequestBody()
      });

      const decoded = response.contentBase64 ? atob(response.contentBase64) : '';
      const bytes = new Uint8Array(decoded.length);
      for (let index = 0; index < decoded.length; index += 1) {
        bytes[index] = decoded.charCodeAt(index);
      }

      const blob = new Blob([bytes], {type: 'text/html;charset=utf-8'});
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = response.fileName || `${this.selectedReportId}.html`;
      link.click();
      URL.revokeObjectURL(link.href);

      this.lastStatus = response.status ?? '';
      this.lastCached = response.isCached ?? false;
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.loading = false;
    }
  }

  private async loadReports(): Promise<void> {
    if (this.id == null) {
      return;
    }

    const definitions = await this.httpClient.request<ReportDefinitionSummary[]>({
      path: '/api/report/get-reports',
      method: 'GET',
      secure: true,
      format: 'json'
    });

    this.reportOptions = definitions.map((item) => ({
      label: item.name || item.id || '',
      value: item.id || ''
    }));

    const fallbackId = this.selectedReportId || this.settings.defaultReportId || definitions[0]?.id || '';
    if (fallbackId) {
      this.selectedReportId = fallbackId;
      await this.loadDefinition(fallbackId);
    }
  }

  private async loadDefinition(reportId: string): Promise<void> {
    const definition = await this.httpClient.request<ReportDefinition>({
      path: '/api/report/get-report-by-id',
      method: 'GET',
      query: {id: reportId},
      secure: true,
      format: 'json'
    });

    this.activeDefinition = definition;
    this.parameters = definition.parameters ?? [];
    this.parameterValues = {};
    for (const parameter of this.parameters) {
      if (!parameter.name) {
        continue;
      }

      this.parameterValues[parameter.name] = parameter.defaultValue || '';
    }
  }

  private createRequestBody() {
    return {
      reportId: this.selectedReportId,
      format: 'Html',
      parameters: this.parameterValues,
      userContext: {
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
      }
    };
  }
}
