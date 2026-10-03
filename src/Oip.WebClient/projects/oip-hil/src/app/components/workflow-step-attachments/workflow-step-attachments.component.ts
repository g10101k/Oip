import { DecimalPipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { MsgService, provideTranslations } from 'oip-common';
import { WorkflowStepApi } from '../../../api/workflow-step.api';
import { UserStepDto, WorkflowAttachment } from '../../../api/data-contracts';
import en from './l10n/workflow-step-attachments.en.json';
import ru from './l10n/workflow-step-attachments.ru.json';

/** Files attached to a workflow step with download buttons; renders nothing when the step has no attachments. */
@Component({
  selector: 'app-workflow-step-attachments',
  providers: [WorkflowStepApi],
  imports: [TranslatePipe, Button, DecimalPipe],
  template: `
    @if (step().attachments?.length) {
      <div class="flex flex-col gap-1">
        <span class="text-surface-500 text-sm">{{ 'workflow-step-attachments.title' | translate }}</span>
        <div class="flex flex-col gap-1">
          @for (attachment of step().attachments; track attachment.stepId + '/' + attachment.fileName) {
            <div class="flex items-center gap-2">
              <i class="pi pi-file text-surface-500"></i>
              <span class="font-mono break-all">{{ attachment.fileName }}</span>
              <span class="text-surface-500 text-sm">{{ (attachment.size ?? 0) / 1024 | number: '1.0-1' }} KB</span>
              <p-button
                icon="pi pi-download"
                size="small"
                [text]="true"
                [ariaLabel]="'workflow-step-attachments.download' | translate"
                (onClick)="download(attachment)" />
            </div>
          }
        </div>
      </div>
    }
  `
})
export class WorkflowStepAttachmentsComponent {
  private readonly translations = provideTranslations({ en, ru });
  private readonly api = inject(WorkflowStepApi);
  private readonly msgService = inject(MsgService);
  private readonly translate = inject(TranslateService);

  readonly step = input.required<UserStepDto>();

  protected async download(attachment: WorkflowAttachment): Promise<void> {
    try {
      const blob = (await this.api.getStepAttachmentByName(
        {
          workflowId: this.step().workflowInstanceId,
          stepId: this.step().id,
          fileName: encodeURIComponent(attachment.fileName ?? '')
        },
        { format: 'blob' }
      )) as unknown as Blob;
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = attachment.fileName ?? '';
      link.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      this.msgService.errorFromException(
        error,
        this.translate.instant('workflow-step-attachments.messages.downloadError')
      );
    }
  }
}
