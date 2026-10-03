import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Message } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { LayoutService, MsgService, provideTranslations } from 'oip-common';
import { WorkflowStepApi } from '../../../api/workflow-step.api';
import { UserStepDto, UserStepStatus } from '../../../api/data-contracts';
import { WorkflowStepEventsService } from '../../service/workflow-step-events.service';
import { WorkflowStepAttachmentsComponent } from '../workflow-step-attachments/workflow-step-attachments.component';
import en from './l10n/workflow-llm-request.en.json';
import ru from './l10n/workflow-llm-request.ru.json';

/** Data the workflow passes to the page (`LlmRequestStep.Data`). */
interface LlmRequestData {
  prompt?: string;
  systemPrompt?: string | null;
  outcomes?: string[];
  model?: string | null;
  /** Request parameters sent to the provider as is, e.g. `temperature`. */
  settings?: Record<string, unknown>;
}

/** Result of the step (`LlmResponse`). */
interface LlmResponseData {
  content?: string | null;
  outcome?: string | null;
  provider?: string;
  model?: string;
  promptTokens?: number | null;
  completionTokens?: number | null;
  elapsedMs?: number;
}

/**
 * Page of an `LlmRequest` workflow step: the workflow performs it itself, so the page is read-only. Shows the prompt,
 * the selected outcome with the reason or the answer, and polls while the request is running.
 * Route: `workflow-llm-request/:workflowId/:stepId`.
 */
@Component({
  selector: 'app-workflow-llm-request',
  providers: [WorkflowStepApi],
  imports: [TranslatePipe, Message, TagModule, DatePipe, WorkflowStepAttachmentsComponent],
  template: `
    <div class="space-y-4" [class.card]="!embedded">
      <div>
        <h5 class="mb-1">
          <i class="pi pi-sparkles"></i> {{ step?.title || ('workflow-llm-request.title' | translate) }}
        </h5>
        <p class="m-0 text-surface-500">{{ 'workflow-llm-request.subtitle' | translate }}</p>
      </div>

      @if (loading && !step) {
        <div class="flex items-center gap-2 text-surface-500">
          <i class="pi pi-spin pi-spinner"></i>
        </div>
      } @else if (!step) {
        <p-message severity="warn" [text]="'workflow-llm-request.notFound' | translate" />
      } @else {
        @if (step.description) {
          <p class="m-0 whitespace-pre-line">{{ step.description }}</p>
        }

        <div class="grid grid-cols-12 gap-2 text-sm">
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-llm-request.workflowInstance' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9 font-mono">{{ step.workflowInstanceId }}</div>
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-llm-request.createdAt' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9">{{ step.createdAt | date: layoutService.dateTimeFormat() }}</div>
          @if (step.completedAt) {
            <div class="col-span-12 md:col-span-3 text-surface-500">
              {{ 'workflow-llm-request.completedAt' | translate }}
            </div>
            <div class="col-span-12 md:col-span-9">{{ step.completedAt | date: layoutService.dateTimeFormat() }}</div>
          }
          @if (response?.provider) {
            <div class="col-span-12 md:col-span-3 text-surface-500">
              {{ 'workflow-llm-request.performer' | translate }}
            </div>
            <div class="col-span-12 md:col-span-9">{{ response?.provider }} · {{ response?.model }}</div>
          }
          @if (response?.elapsedMs !== undefined) {
            <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-llm-request.usage' | translate }}</div>
            <div class="col-span-12 md:col-span-9">
              {{
                'workflow-llm-request.usageValue'
                  | translate
                    : {
                        prompt: response?.promptTokens ?? '-',
                        completion: response?.completionTokens ?? '-',
                        elapsed: response?.elapsedMs
                      }
              }}
            </div>
          }
        </div>

        @if (request.systemPrompt) {
          <div class="flex flex-col gap-1">
            <span class="text-surface-500 text-sm">{{ 'workflow-llm-request.systemPrompt' | translate }}</span>
            <div class="rounded-border bg-surface-100 dark:bg-surface-800 p-3 whitespace-pre-wrap">
              {{ request.systemPrompt }}
            </div>
          </div>
        }
        <div class="flex flex-col gap-1">
          <span class="text-surface-500 text-sm">{{ 'workflow-llm-request.prompt' | translate }}</span>
          <div class="rounded-border bg-surface-100 dark:bg-surface-800 p-3 whitespace-pre-wrap">
            {{ request.prompt }}
          </div>
        </div>

        @if (settings.length) {
          <div class="flex flex-col gap-1">
            <span class="text-surface-500 text-sm">{{ 'workflow-llm-request.settings' | translate }}</span>
            <div class="grid grid-cols-12 gap-x-2 gap-y-1 text-sm">
              @for (setting of settings; track setting.key) {
                <div class="col-span-12 md:col-span-3 font-mono">{{ setting.key }}</div>
                <div class="col-span-12 md:col-span-9 font-mono break-all">{{ setting.value }}</div>
              }
            </div>
          </div>
        }

        @if (request.outcomes?.length) {
          <div class="flex flex-col gap-1">
            <span class="text-surface-500 text-sm">{{ 'workflow-llm-request.outcomes' | translate }}</span>
            <div class="flex flex-wrap gap-2">
              @for (outcome of request.outcomes; track outcome) {
                <p-tag
                  [icon]="outcome === response?.outcome ? 'pi pi-check' : ''"
                  [severity]="outcome === response?.outcome ? 'success' : 'secondary'"
                  [value]="outcome" />
              }
            </div>
          </div>
        }

        @switch (step.status) {
          @case (UserStepStatus.Running) {
            <div class="flex items-center gap-2 text-surface-500">
              <i class="pi pi-spin pi-spinner"></i>{{ 'workflow-llm-request.running' | translate }}
            </div>
          }
          @case (UserStepStatus.Failed) {
            <p-message severity="error">
              <div class="flex flex-col gap-1">
                <span class="font-medium">{{ 'workflow-llm-request.failed' | translate }}</span>
                <span class="whitespace-pre-wrap">{{ step.error }}</span>
              </div>
            </p-message>
          }
          @case (UserStepStatus.Cancelled) {
            <p-message severity="secondary" [text]="'workflow-llm-request.cancelled' | translate" />
          }
          @default {
            @if (response?.content) {
              <div class="flex flex-col gap-1">
                <span class="text-surface-500 text-sm">
                  {{
                    (response?.outcome ? 'workflow-llm-request.reason' : 'workflow-llm-request.response') | translate
                  }}
                </span>
                <div class="rounded-border border border-surface p-3 whitespace-pre-wrap">{{ response?.content }}</div>
              </div>
            }
            <app-workflow-step-attachments [step]="step" />
          }
        }
      }
    </div>
  `
})
export class WorkflowLlmRequestComponent implements OnInit {
  private readonly translations = provideTranslations({ en, ru });
  private readonly api = inject(WorkflowStepApi);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly msgService = inject(MsgService);
  private readonly translate = inject(TranslateService);
  private readonly titleService = inject(Title);
  private readonly stepEvents = inject(WorkflowStepEventsService);

  private static readonly pollInterval = 3000;
  private pollTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly layoutService = inject(LayoutService);
  /** Rendered inside the workflow activity module card, so it has no card of its own. */
  protected readonly embedded = this.route.snapshot.data['embedded'] === true;
  protected readonly UserStepStatus = UserStepStatus;

  protected workflowId = '';
  protected stepId = '';
  protected step: UserStepDto | null = null;
  protected request: LlmRequestData = {};
  protected response: LlmResponseData | null = null;
  /** Request settings as `key` / JSON `value` pairs. */
  protected settings: { key: string; value: string }[] = [];
  protected loading = true;

  constructor() {
    this.destroyRef.onDestroy(() => this.stopPolling());
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.workflowId = params.get('workflowId') ?? '';
      this.stepId = params.get('stepId') ?? '';
      this.step = null;
      void this.loadStep();
    });
  }

  protected async loadStep(): Promise<void> {
    this.stopPolling();
    const wasRunning = this.step?.status === UserStepStatus.Running;
    this.loading = true;
    try {
      this.step = await this.api.getStepById({ workflowId: this.workflowId, stepId: this.stepId });
      this.request = JSON.parse(this.step.data ?? '{}') as LlmRequestData;
      this.settings = Object.entries(this.request.settings ?? {}).map(([key, value]) => ({
        key,
        value: JSON.stringify(value)
      }));
      this.response = this.step.result ? (JSON.parse(this.step.result) as LlmResponseData) : null;
      this.titleService.setTitle(this.step.title || this.translate.instant('workflow-llm-request.title'));

      if (this.step.status === UserStepStatus.Running) {
        this.pollTimer = setTimeout(() => void this.loadStep(), WorkflowLlmRequestComponent.pollInterval);
      } else if (wasRunning) {
        // The workflow went on after the request, so the task list may have new steps.
        this.stepEvents.notifyCompleted({
          workflowInstanceId: this.step.workflowInstanceId,
          status: this.step.workflowStatus
        });
      }
    } catch (error) {
      this.step = null;
      this.request = {};
      this.settings = [];
      this.response = null;
      if (!this.isNotFound(error)) {
        this.msgService.errorFromException(error, this.translate.instant('workflow-llm-request.messages.loadError'));
      }
    } finally {
      this.loading = false;
    }
  }

  private stopPolling(): void {
    if (this.pollTimer) {
      clearTimeout(this.pollTimer);
      this.pollTimer = null;
    }
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && (error as { status?: number }).status === 404;
  }
}
