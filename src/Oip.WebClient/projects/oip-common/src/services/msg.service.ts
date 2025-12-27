import { inject, Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastMessageOptions } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class MsgService {
  protected readonly messageService: MessageService = inject(MessageService);
  private readonly translate = inject(TranslateService);
  private readonly lifetime: number = 2000;

  add(msg: ToastMessageOptions) {
    this.messageService.add(msg);
  }

  success(detail: any, summary: any = this.translate.instant('msgService.success'), life: number = this.lifetime) {
    this.messageService.add({
      severity: 'success',
      summary: summary,
      detail: detail,
      life: life
    });
  }

  info(detail: any, summary: any = this.translate.instant('msgService.info'), life: number = this.lifetime) {
    this.messageService.add({
      severity: 'info',
      summary: summary,
      detail: detail,
      life: life
    });
  }

  warn(detail: any, summary: any = this.translate.instant('msgService.warn'), life: number = this.lifetime) {
    this.messageService.add({
      severity: 'warn',
      summary: summary,
      detail: detail,
      life: life
    });
  }

  error(detail: any, summary: any = this.translate.instant('msgService.error'), life: number = this.lifetime) {
    if (detail instanceof HttpErrorResponse) {
      summary = `Error: ${detail.status} ${detail.statusText}`;
      detail = `${detail.name} \r\n ${detail.message}`;
    }
    this.messageService.add({
      severity: 'error',
      summary: summary,
      detail: detail.toString(),
      life: life
    });
  }

  contrast(detail: any, summary: any = this.translate.instant('msgService.error'), life: number = this.lifetime) {
    this.messageService.add({
      severity: 'contrast',
      summary: summary,
      detail: detail,
      life: life
    });
  }

  secondary(detail: any, summary: any = this.translate.instant('msgService.secondary'), life: number = this.lifetime) {
    this.messageService.add({
      severity: 'secondary',
      summary: summary,
      detail: detail,
      life: life
    });
  }
}
