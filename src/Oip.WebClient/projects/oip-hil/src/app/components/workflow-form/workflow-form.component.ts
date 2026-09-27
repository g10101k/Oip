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
import en from './l10n/workflow-form.en.json';
import ru from './l10n/workflow-form.ru.json';

/**
 * Page of a `UserForm` workflow step: renders an input per field and resumes the Temporal workflow on submit.
 * A submitted or cancelled form is shown read-only.
 * Route: `workflow-form/:workflowId/:stepId`.
 */
@Component({
  selector: 'app-workflow-form',
  providers: [WorkflowStepApi],
  imports: [TranslatePipe, Button, InputText, Textarea, Message, FormsModule, DatePipe],
  template: `
    <div class="space-y-4" [class.card]="!embedded">
      <div>
        <h5 class="mb-1"><i class="pi pi-file-edit"></i> {{ form?.title || ('workflow-form.title' | translate) }}</h5>
        <p class="m-0 text-surface-500">{{ 'workflow-form.subtitle' | translate }}</p>
      </div>

      @if (loading) {
        <div class="flex items-center gap-2 text-surface-500">
          <i class="pi pi-spin pi-spinner"></i>
        </div>
      } @else if (!form) {
        <p-message severity="warn" [text]="'workflow-form.notFound' | translate" />
      } @else {
        @if (form.description) {
          <p class="m-0 whitespace-pre-line">{{ form.description }}</p>
        }

        <div class="grid grid-cols-12 gap-2 text-sm">
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-form.workflowInstance' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9 font-mono">{{ form.workflowInstanceId }}</div>
          <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-form.createdAt' | translate }}</div>
          <div class="col-span-12 md:col-span-9">{{ form.createdAt | date: layoutService.dateTimeFormat() }}</div>
          @if (form.completedAt) {
            <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-form.submittedAt' | translate }}</div>
            <div class="col-span-12 md:col-span-9">
              {{ form.completedAt | date: layoutService.dateTimeFormat() }} · {{ form.completedBy }}
            </div>
          }
        </div>

        @if (form.status === UserStepStatus.Completed) {
          <div class="grid grid-cols-12 gap-2">
            @for (field of fields; track field) {
              <div class="col-span-12 md:col-span-3 text-surface-500">{{ field }}</div>
              <div class="col-span-12 md:col-span-9 font-medium">{{ values[field] }}</div>
            }
            @if (form.comment) {
              <div class="col-span-12 md:col-span-3 text-surface-500">{{ 'workflow-form.comment' | translate }}</div>
              <div class="col-span-12 md:col-span-9 whitespace-pre-line">{{ form.comment }}</div>
            }
          </div>
        } @else if (form.status === UserStepStatus.Cancelled) {
          <p-message severity="secondary" [text]="'workflow-form.cancelled' | translate" />
        } @else {
          <div class="flex flex-col gap-4 max-w-xl">
            @for (field of fields; track field) {
              <div class="flex flex-col gap-2">
                <label [for]="'field-' + field">{{ field }} <span class="text-red-500">*</span></label>
                <input [id]="'field-' + field" pInputText [disabled]="saving" [(ngModel)]="values[field]" />
              </div>
            }

            <div class="flex flex-col gap-2">
              <label for="formComment">{{ 'workflow-form.comment' | translate }}</label>
              <textarea
                id="formComment"
                pTextarea
                rows="3"
                [placeholder]="'workflow-form.commentPlaceholder' | translate"
                [disabled]="saving"
                [(ngModel)]="comment"></textarea>
            </div>

            <div class="flex justify-end">
              <p-button
                icon="pi pi-send"
                [label]="'workflow-form.submit' | translate"
                [loading]="saving"
                [disabled]="!isComplete"
                (onClick)="submit()" />
            </div>
          </div>
        }
      }
    </div>
  `
})
export class WorkflowFormComponent implements OnInit {
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
  protected form: UserStepDto | null = null;
  protected fields: string[] = [];
  protected values: Record<string, string> = {};
  protected comment = '';
  protected loading = true;
  protected saving = false;

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.workflowId = params.get('workflowId') ?? '';
      this.stepId = params.get('stepId') ?? '';
      void this.loadForm();
    });
  }

  protected get isComplete(): boolean {
    return this.fields.every((field) => (this.values[field] ?? '').trim() !== '');
  }

  protected async loadForm(): Promise<void> {
    this.loading = true;
    this.comment = '';
    try {
      this.form = await this.api.getStepById({ workflowId: this.workflowId, stepId: this.stepId });
      this.fields = (JSON.parse(this.form.data ?? '{}') as { fields?: string[] }).fields ?? [];
      const submitted = JSON.parse(this.form.result ?? '{}') as Record<string, string>;
      this.values = this.toValues((field) => submitted[field] ?? '');
      this.titleService.setTitle(this.form.title || this.translate.instant('workflow-form.title'));
    } catch (error) {
      this.form = null;
      this.fields = [];
      this.values = {};
      if (!this.isNotFound(error)) {
        this.msgService.errorFromException(error, this.translate.instant('workflow-form.messages.loadError'));
      }
    } finally {
      this.loading = false;
    }
  }

  protected async submit(): Promise<void> {
    if (!this.form) return;
    if (!this.isComplete) {
      this.msgService.error(this.translate.instant('workflow-form.messages.required'));
      return;
    }

    this.saving = true;
    try {
      const values = this.toValues((field) => this.values[field].trim());
      const submitted = await this.api.completeStep(
        { workflowId: this.workflowId, stepId: this.stepId },
        { result: JSON.stringify(values), comment: this.comment.trim() || null }
      );
      this.msgService.success(this.translate.instant('workflow-form.submitted', { status: submitted.status }));
      this.stepEvents.notifyCompleted(submitted);
      await this.loadForm();
    } catch (error) {
      if (this.isNotFound(error)) {
        this.form = null;
      } else {
        this.msgService.errorFromException(error, this.translate.instant('workflow-form.messages.submitError'));
      }
    } finally {
      this.saving = false;
    }
  }

  private toValues(valueOf: (field: string) => string): Record<string, string> {
    const values: Record<string, string> = {};
    for (const field of this.fields) values[field] = valueOf(field);
    return values;
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && (error as { status?: number }).status === 404;
  }
}
