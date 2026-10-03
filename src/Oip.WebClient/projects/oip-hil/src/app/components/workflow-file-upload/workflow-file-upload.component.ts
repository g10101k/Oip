import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { FileUpload, FileUploadHandlerEvent } from 'primeng/fileupload';
import { Message } from 'primeng/message';
import { LayoutService, MsgService, provideTranslations } from 'oip-common';
import { WorkflowStepApi } from '../../../api/workflow-step.api';
import { UserStepDto, UserStepStatus } from '../../../api/data-contracts';
import { WorkflowStepEventsService } from '../../service/workflow-step-events.service';
import { WorkflowStepAttachmentsComponent } from '../workflow-step-attachments/workflow-step-attachments.component';
import en from './l10n/workflow-file-upload.en.json';
import ru from './l10n/workflow-file-upload.ru.json';

/** Data the workflow passes to the page (`UserFileUploadStep.Data`). */
interface FileUploadData {
  /** Allowed extensions with the leading dot; empty for any file. */
  extensions?: string[];
  /** Maximum size in bytes; `null` for the storage limit. */
  maxFileSize?: number | null;
}

/**
 * Page of a `UserFileUpload` workflow step: uploads a file to the step folder, which completes the step and resumes
 * the Temporal workflow. A completed step shows the uploaded file.
 * Route: `workflow-file-upload/:workflowId/:stepId`.
 */
@Component({
  selector: 'app-workflow-file-upload',
  providers: [WorkflowStepApi],
  imports: [TranslatePipe, FileUpload, Message, DatePipe, DecimalPipe, WorkflowStepAttachmentsComponent],
  template: `
    <div class="space-y-4" [class.card]="!embedded">
      <div>
        <h5 class="mb-1">
          <i class="pi pi-upload"></i> {{ step?.title || ('workflow-file-upload.title' | translate) }}
        </h5>
        <p class="m-0 text-surface-500">{{ 'workflow-file-upload.subtitle' | translate }}</p>
      </div>

      @if (loading) {
        <div class="flex items-center gap-2 text-surface-500">
          <i class="pi pi-spin pi-spinner"></i>
        </div>
      } @else if (!step) {
        <p-message severity="warn" [text]="'workflow-file-upload.notFound' | translate" />
      } @else {
        @if (step.description) {
          <p class="m-0 whitespace-pre-line">{{ step.description }}</p>
        }

        <div class="grid grid-cols-12 gap-2 text-sm">
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-file-upload.workflowInstance' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9 font-mono">{{ step.workflowInstanceId }}</div>
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-file-upload.createdAt' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9">{{ step.createdAt | date: layoutService.dateTimeFormat() }}</div>
          @if (step.completedAt) {
            <div class="col-span-12 md:col-span-3 text-surface-500">
              {{ 'workflow-file-upload.completedAt' | translate }}
            </div>
            <div class="col-span-12 md:col-span-9">
              {{ step.completedAt | date: layoutService.dateTimeFormat() }} · {{ step.completedBy }}
            </div>
          }
          <div class="col-span-12 md:col-span-3 text-surface-500">
            {{ 'workflow-file-upload.allowedExtensions' | translate }}
          </div>
          <div class="col-span-12 md:col-span-9 font-mono">
            {{ data.extensions?.length ? data.extensions?.join(', ') : ('workflow-file-upload.anyFile' | translate) }}
          </div>
          @if (data.maxFileSize) {
            <div class="col-span-12 md:col-span-3 text-surface-500">
              {{ 'workflow-file-upload.maxFileSize' | translate }}
            </div>
            <div class="col-span-12 md:col-span-9">{{ data.maxFileSize / 1024 | number: '1.0-0' }} KB</div>
          }
        </div>

        @if (step.status === UserStepStatus.Completed) {
          <app-workflow-step-attachments [step]="step" />
        } @else if (step.status === UserStepStatus.Cancelled) {
          <p-message severity="secondary" [text]="'workflow-file-upload.cancelled' | translate" />
        } @else {
          <div class="flex items-center gap-2">
            <p-fileupload
              chooseIcon="pi pi-upload"
              mode="basic"
              name="file"
              [accept]="data.extensions?.join(',') ?? ''"
              [auto]="true"
              [chooseLabel]="'workflow-file-upload.choose' | translate"
              [customUpload]="true"
              [disabled]="saving"
              [maxFileSize]="data.maxFileSize ?? undefined"
              (uploadHandler)="upload($event)" />
            @if (saving) {
              <i class="pi pi-spin pi-spinner text-surface-500"></i>
            }
          </div>
        }
      }
    </div>
  `
})
export class WorkflowFileUploadComponent implements OnInit {
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
  protected step: UserStepDto | null = null;
  protected data: FileUploadData = {};
  protected loading = true;
  protected saving = false;

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.workflowId = params.get('workflowId') ?? '';
      this.stepId = params.get('stepId') ?? '';
      void this.loadStep();
    });
  }

  protected async loadStep(): Promise<void> {
    this.loading = true;
    try {
      this.step = await this.api.getStepById({ workflowId: this.workflowId, stepId: this.stepId });
      this.data = JSON.parse(this.step.data ?? '{}') as FileUploadData;
      this.titleService.setTitle(this.step.title || this.translate.instant('workflow-file-upload.title'));
    } catch (error) {
      this.step = null;
      this.data = {};
      if (!this.isNotFound(error)) {
        this.msgService.errorFromException(error, this.translate.instant('workflow-file-upload.messages.loadError'));
      }
    } finally {
      this.loading = false;
    }
  }

  protected async upload(event: FileUploadHandlerEvent): Promise<void> {
    const file = event.files?.[0];
    if (!file || !this.step) return;

    this.saving = true;
    try {
      const completed = await this.api.uploadStepFile(
        { workflowId: this.workflowId, stepId: this.stepId },
        { File: file }
      );
      this.msgService.success(this.translate.instant('workflow-file-upload.uploaded', { status: completed.status }));
      this.stepEvents.notifyCompleted(completed);
      await this.loadStep();
    } catch (error) {
      if (this.isNotFound(error)) {
        this.step = null;
      } else {
        this.msgService.errorFromException(error, this.translate.instant('workflow-file-upload.messages.uploadError'));
      }
    } finally {
      this.saving = false;
    }
  }

  private isNotFound(error: unknown): boolean {
    return typeof error === 'object' && error !== null && (error as { status?: number }).status === 404;
  }
}
