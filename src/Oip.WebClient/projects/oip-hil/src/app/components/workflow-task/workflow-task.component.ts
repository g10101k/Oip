import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Textarea } from 'primeng/textarea';
import { LayoutService, MsgService, provideTranslations } from 'oip-common';
import { WorkflowStepApi } from '../../../api/workflow-step.api';
import { WorkflowStepEventsService } from '../../service/workflow-step-events.service';
import { UserStepDto, UserStepStatus } from '../../../api/data-contracts';
import en from './l10n/workflow-task.en.json';
import ru from './l10n/workflow-task.ru.json';

/**
 * Page of a `UserTask` workflow step: shows the task and resumes the Temporal workflow when it is completed.
 * A completed or cancelled task is shown read-only.
 * Route: `workflow-task/:workflowId/:stepId`.
 */
@Component({
  selector: 'app-workflow-task',
  providers: [WorkflowStepApi],
  imports: [TranslatePipe, Button, InputText, Textarea, Message, FormsModule, DatePipe],
  template: `
    <div class="space-y-4" [class.card]="!embedded">
      <div>
        <h5 class="mb-1">
          <i class="pi pi-check-square"></i> {{ task?.title || ('workflow-task.title' | translate) }}
        </h5>
        <p class="m-0 text-surface-500">{{ 'workflow-task.subtitle' | translate }}</p>
      </div>

      @if (loading) {
        <div class="flex items-center gap-2 text-surface-500">
          <i class="pi pi-spin pi-spinner"></i>
        </div>
      } @else if (!task) {
        <p-message severity="warn" [text]="'workflow-task.notFound' | translate" />
      } @else {
        @if (task.description) {
          <p class="m-0 whitespace-pre-line">{{ task.description }}</p>
        }

        <div class="grid grid-cols-12 gap-2 text-sm">
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-task.workflowInstance' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9 font-mono">{{ task.workflowInstanceId }}</div>
          <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-task.createdAt' | translate }}</div>
          <div class="col-span-12 md:col-span-9">{{ task.createdAt | date: layoutService.dateTimeFormat() }}</div>
          @if (task.completedAt) {
            <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-task.completedAt' | translate }}</div>
            <div class="col-span-12 md:col-span-9">
              {{ task.completedAt | date: layoutService.dateTimeFormat() }} · {{ task.completedBy }}
            </div>
          }
        </div>

        @if (task.status === UserStepStatus.Completed) {
          <div class="grid grid-cols-12 gap-2">
            <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-task.result' | translate }}</div>
            <div class="col-span-12 md:col-span-9 font-medium">{{ completedResult }}</div>
            @if (task.comment) {
              <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-task.comment' | translate }}</div>
              <div class="col-span-12 md:col-span-9 whitespace-pre-line">{{ task.comment }}</div>
            }
          </div>
        } @else if (task.status === UserStepStatus.Cancelled) {
          <p-message severity="secondary" [text]="'workflow-task.cancelled' | translate" />
        } @else {
          <div class="flex flex-col gap-4 max-w-xl">
            @if (outcomes.length) {
              <div class="flex flex-col gap-2">
                <span>{{ 'workflow-task.outcome' | translate }}</span>
                <div class="flex flex-wrap gap-2">
                  @for (outcome of outcomes; track outcome) {
                    <p-button
                      [label]="outcome"
                      [outlined]="result !== outcome"
                      [disabled]="saving"
                      (onClick)="result = outcome" />
                  }
                </div>
              </div>
            } @else {
              <div class="flex flex-col gap-2">
                <label for="taskResult">{{ 'workflow-task.result' | translate }}</label>
                <input
                  id="taskResult"
                  pInputText
                  [placeholder]="'workflow-task.resultPlaceholder' | translate"
                  [disabled]="saving"
                  [(ngModel)]="result" />
              </div>
            }

            <div class="flex flex-col gap-2">
              <label for="taskComment">{{ 'workflow-task.comment' | translate }}</label>
              <textarea
                id="taskComment"
                pTextarea
                rows="3"
                [placeholder]="'workflow-task.commentPlaceholder' | translate"
                [disabled]="saving"
                [(ngModel)]="comment"></textarea>
            </div>

            <div class="flex justify-end">
              <p-button
                icon="pi pi-check"
                [label]="'workflow-task.complete' | translate"
                [loading]="saving"
                [disabled]="outcomes.length > 0 && !result"
                (onClick)="complete()" />
            </div>
          </div>
        }
      }
    </div>
  `
})
export class WorkflowTaskComponent implements OnInit {
  private readonly translations = provideTranslations({ en, ru });
  private readonly api = inject(WorkflowStepApi);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly msgService = inject(MsgService);
  private readonly translate = inject(TranslateService);
  private readonly titleService = inject(Title);
  private readonly stepEvents = inject(WorkflowStepEventsService);

  protected readonly layoutService = inject(LayoutService);
  /** Rendered inside the workflow activity module card, so it has no card of its own. */
  protected readonly embedded = this.route.snapshot.data['embedded'] === true;
  protected readonly UserStepStatus = UserStepStatus;

  protected workflowId = '';
  protected stepId = '';
  protected task: UserStepDto | null = null;
  protected outcomes: string[] = [];
  /** Result of a completed task. */
  protected completedResult = '';
  protected result = '';
  protected comment = '';
  protected loading = true;
  protected saving = false;

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.workflowId = params.get('workflowId') ?? '';
      this.stepId = params.get('stepId') ?? '';
      void this.loadTask();
    });
  }

  protected async loadTask(): Promise<void> {
    this.loading = true;
    this.result = '';
    this.comment = '';
    try {
      this.task = await this.api.getStepById({ workflowId: this.workflowId, stepId: this.stepId });
      this.outcomes = (JSON.parse(this.task.data ?? '{}') as { outcomes?: string[] }).outcomes ?? [];
      this.completedResult = this.task.result ? String(JSON.parse(this.task.result)) : '';
      this.titleService.setTitle(this.task.title || this.translate.instant('workflow-task.title'));
    } catch (error) {
      this.task = null;
      this.outcomes = [];
      if (!this.isNotFound(error)) {
        this.msgService.errorFromException(error, this.translate.instant('workflow-task.messages.loadError'));
      }
    } finally {
      this.loading = false;
    }
  }

  protected async complete(): Promise<void> {
    if (!this.task) return;
    if (this.outcomes.length && !this.result) {
      this.msgService.error(this.translate.instant('workflow-task.messages.outcomeRequired'));
      return;
    }

    this.saving = true;
    try {
      const completed = await this.api.completeStep(
        { workflowId: this.workflowId, stepId: this.stepId },
        { result: JSON.stringify(this.result.trim()), comment: this.comment.trim() || null }
      );
      this.msgService.success(this.translate.instant('workflow-task.completed', { status: completed.status }));
      this.stepEvents.notifyCompleted(completed);
      await this.loadTask();
    } catch (error) {
      if (this.isNotFound(error)) {
        this.task = null;
      } else {
        this.msgService.errorFromException(error, this.translate.instant('workflow-task.messages.completeError'));
      }
    } finally {
      this.saving = false;
    }
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && (error as { status?: number }).status === 404;
  }
}
