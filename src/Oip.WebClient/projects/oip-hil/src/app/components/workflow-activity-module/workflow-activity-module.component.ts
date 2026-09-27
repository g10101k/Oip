import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { InputText } from 'primeng/inputtext';
import { Splitter } from 'primeng/splitter';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { Tooltip } from 'primeng/tooltip';
import { BaseModuleComponent, NoSettingsDto, provideTranslations, SecurityComponent } from 'oip-common';
import { WorkflowActivityModuleApi } from '../../../api/workflow-activity-module.api';
import { WorkflowDemoApi } from '../../../api/workflow-demo.api';
import { UserStepDto, UserStepStatus, WorkflowActivityModuleSettings } from '../../../api/data-contracts';
import { WorkflowStepEventsService } from '../../service/workflow-step-events.service';
import en from './l10n/workflow-activity-module.en.json';
import ru from './l10n/workflow-activity-module.ru.json';

/**
 * Lists user steps, pending and completed, of the Temporal workflows started in the selected period. Selecting a row
 * opens the step in the child `router-outlet` of the splitter panel next to the table; which component renders it is
 * decided by the step routes in `app.routes.ts`.
 */
@Component({
  selector: 'app-workflow-activity-module',
  providers: [WorkflowActivityModuleApi, WorkflowDemoApi],
  imports: [
    SecurityComponent,
    TranslatePipe,
    Button,
    DatePicker,
    InputText,
    Splitter,
    TableModule,
    TagModule,
    ToolbarModule,
    Tooltip,
    RouterOutlet,
    FormsModule,
    DatePipe
  ],
  template: `
    @if (isContent) {
      <!-- Fills the screen below the app topbar: 5rem top padding of the layout + 1rem bottom margin of the card. -->
      <div class="card flex h-[calc(100dvh-6rem)] flex-col">
        <p-toolbar class="mb-4 shrink-0">
          <div class="flex flex-col md:flex-row md:items-center gap-2 w-full">
            <div class="font-semibold text-lg flex items-center gap-2 mx-2">
              <i class="pi pi-sitemap"></i>
              {{ title }}
            </div>

            <div class="flex-1"></div>
            <div class="flex items-center gap-1 w-full md:w-auto">
              <p-datepicker
                appendTo="body"
                hourFormat="24"
                selectionMode="range"
                styleClass="w-full md:w-96"
                [dateFormat]="layoutService.primeNgDateFormat()"
                [fluid]="true"
                [placeholder]="'workflow-activity-module.content.period' | translate"
                [readonlyInput]="true"
                [showIcon]="true"
                [showTime]="true"
                [(ngModel)]="period"
                (onClose)="onPeriodClose()" />
              <p-button
                icon="pi pi-refresh"
                rounded="true"
                severity="secondary"
                text="true"
                tooltipPosition="bottom"
                [loading]="loading"
                [pTooltip]="'workflow-activity-module.content.refresh' | translate"
                (onClick)="loadTasks()" />
              <p-button
                icon="pi pi-play"
                rounded="true"
                severity="success"
                text="true"
                tooltipPosition="bottom"
                [disabled]="loading || !canRead"
                [loading]="startingDemo"
                [pTooltip]="'workflow-activity-module.content.runDemo' | translate"
                (onClick)="runDemo()" />
            </div>
          </div>
        </p-toolbar>

        <p-splitter
          panelStyleClass="overflow-auto"
          stateKey="workflow-activity-module-splitter"
          stateStorage="local"
          styleClass="!border-0 min-h-0 flex-1"
          [minSizes]="[20, 20]"
          [panelSizes]="[50, 50]">
          <ng-template #panel>
            <div class="flex h-full w-full flex-col pr-4">
              <p-table
                dataKey="id"
                scrollHeight="flex"
                [loading]="loading"
                [rowHover]="true"
                [scrollable]="true"
                [value]="tasks">
                <ng-template pTemplate="header">
                  <tr>
                    <th>{{ 'workflow-activity-module.content.table.title' | translate }}</th>
                    <th>{{ 'workflow-activity-module.content.table.description' | translate }}</th>
                    <th>{{ 'workflow-activity-module.content.table.createdAt' | translate }}</th>
                    <th>{{ 'workflow-activity-module.content.table.status' | translate }}</th>
                  </tr>
                </ng-template>

                <ng-template let-task pTemplate="body">
                  <tr
                    class="cursor-pointer"
                    [class.p-datatable-row-selected]="task.id === selectedTaskId"
                    (click)="openTask(task)">
                    <td>{{ task.title || task.id }}</td>
                    <td>{{ task.description }}</td>
                    <td>{{ task.createdAt | date: layoutService.dateTimeFormat() }}</td>
                    <td>
                      <p-tag
                        [severity]="statusSeverity(task.status)"
                        [value]="'workflow-activity-module.content.status.' + task.status | translate" />
                      @if (task.completedBy) {
                        <div class="mt-1">{{ task.completedBy }}</div>
                      }
                    </td>
                  </tr>
                </ng-template>

                <ng-template pTemplate="emptymessage">
                  <tr>
                    <td class="py-8 text-center text-surface-500" colspan="4">
                      {{ 'workflow-activity-module.content.empty' | translate }}
                    </td>
                  </tr>
                </ng-template>
              </p-table>
            </div>
          </ng-template>

          <ng-template #panel>
            <div class="h-full w-full pl-4">
              <router-outlet (activate)="stepOpen = true" (deactivate)="stepOpen = false" />
              @if (!stepOpen) {
                <div class="text-surface-500">
                  <i class="pi pi-arrow-left mr-2"></i>{{ 'workflow-activity-module.content.selectHint' | translate }}
                </div>
              }
            </div>
          </ng-template>
        </p-splitter>
      </div>
    } @else if (isSettings) {
      <div class="flex flex-col md:flex-row gap-8">
        <div class="md:w-1/2">
          <div class="card flex flex-col gap-4">
            <div class="font-semibold text-xl">{{ 'workflow-activity-module.settings.title' | translate }}</div>
            <div class="grid grid-cols-12 gap-4">
              <label class="flex items-center col-span-12 mb-2 md:col-span-2 md:mb-0" for="dayCount">
                {{ 'workflow-activity-module.settings.dayCount' | translate }}
              </label>
              <div class="col-span-12 md:col-span-10">
                <input id="dayCount" pInputText type="number" [(ngModel)]="settings.dayCount" />
              </div>
            </div>
            <div class="flex justify-end">
              <p-button
                icon="pi pi-save"
                [label]="'workflow-activity-module.settings.save' | translate"
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
export class WorkflowActivityModuleComponent extends BaseModuleComponent<
  WorkflowActivityModuleSettings,
  NoSettingsDto
> {
  private readonly translations = provideTranslations({ en, ru });
  private readonly api = inject(WorkflowActivityModuleApi);
  private readonly demoApi = inject(WorkflowDemoApi);
  private readonly stepEvents = inject(WorkflowStepEventsService);
  private readonly router = inject(Router);

  protected override hideFooter = true;

  protected tasks: UserStepDto[] = [];
  /** Period of the workflow start: `[from, to]`, `to` is `null` while the range is being selected. */
  protected period: (Date | null)[] | null = null;
  protected loading = false;
  protected startingDemo = false;
  /** True while a step is rendered in the child router outlet. */
  protected stepOpen = false;
  /** Id of the step opened in the child route, used to highlight its row. */
  protected selectedTaskId: string | null = null;

  constructor() {
    super();
    this.l10nService.get('workflow-activity-module').subscribe((l10n) => {
      this.appTitleService.setTitle(l10n.title);
    });
    this.stepEvents.completed$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => void this.loadTasks());
    // `routerLinkActive` is not used on purpose: inside `p-table` it hangs the page (infinite change detection).
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.updateSelectedTask());
    this.updateSelectedTask();
  }

  private updateSelectedTask(): void {
    const segments = this.route.firstChild?.snapshot.url ?? [];
    this.selectedTaskId = segments.length ? segments[segments.length - 1].path : null;
  }

  protected override async onModuleInstanceChange(): Promise<void> {
    this.period = this.defaultPeriod();
    await this.loadTasks();
  }

  /** Reloads the tasks when the picker closes; a single selected date means the whole day. */
  protected async onPeriodClose(): Promise<void> {
    const [from, to] = this.period ?? [];
    if (!from) {
      this.period = this.defaultPeriod();
    } else if (!to) {
      this.period = [from, this.endOfDay(from)];
    }
    await this.loadTasks();
  }

  protected statusSeverity(status: UserStepStatus): 'warn' | 'success' | 'secondary' {
    switch (status) {
      case UserStepStatus.Pending:
        return 'warn';
      case UserStepStatus.Completed:
        return 'success';
      default:
        return 'secondary';
    }
  }

  /** Last `dayCount` days from the module settings, including today. */
  private defaultPeriod(): Date[] {
    const to = this.endOfDay(new Date());
    const from = new Date(to);
    from.setDate(from.getDate() - Math.max((this.settings?.dayCount ?? 5) - 1, 0));
    from.setHours(0, 0, 0, 0);
    return [from, to];
  }

  private endOfDay(date: Date): Date {
    const end = new Date(date);
    end.setHours(23, 59, 59, 999);
    return end;
  }

  /** Opens the step in the child outlet via the route of the page that renders it. */
  protected openTask(task: UserStepDto): void {
    void this.router.navigate([task.route ?? '', task.workflowInstanceId ?? '', task.id ?? ''], {
      relativeTo: this.route
    });
  }

  protected async loadTasks(): Promise<void> {
    if (this.securityRightsLoaded && !this.canRead) {
      this.tasks = [];
      return;
    }

    this.loading = true;
    try {
      const [from, to] = this.period ?? this.defaultPeriod();
      this.tasks = await this.api.getStepsByPeriod({ from: from ?? undefined, to: to ?? undefined });
    } catch (error) {
      this.tasks = [];
      this.msgService.errorFromException(error, String(this.t('workflow-activity-module.messages.loadError')));
    } finally {
      this.loading = false;
    }
  }

  protected async runDemo(): Promise<void> {
    this.startingDemo = true;
    try {
      await this.demoApi.runUserTaskDemo();
      this.msgService.success(this.t('workflow-activity-module.messages.demoStarted'));
      await this.loadTasks();
    } catch (error) {
      this.msgService.errorFromException(error, String(this.t('workflow-activity-module.messages.demoError')));
    } finally {
      this.startingDemo = false;
    }
  }
}
