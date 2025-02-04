import { inject, Injectable } from '@angular/core';
import { MessageService } from "primeng/api";
import { HttpErrorResponse } from "@angular/common/http";
import { ToastMessageOptions } from "primeng/api/toastmessage";

@Injectable({
  providedIn: 'root'
})
export class MsgService {
  protected readonly messageService: MessageService = inject(MessageService);
  private readonly lifetime: number = 2000;

  add(msg: ToastMessageOptions) {
    this.messageService.add(msg);
  }

  success(detail: any, summary: any = 'Success', life: number = this.lifetime) {
    this.messageService.add({ severity: 'success', summary: summary, detail: detail, life: life });
  }

  info(detail: any, summary: any = 'Info', life: number = this.lifetime) {
    this.messageService.add({ severity: 'info', summary: summary, detail: detail, life: life });
  }

  warn(detail: any, summary: any = 'Warn', life: number = this.lifetime) {
    this.messageService.add({ severity: 'warn', summary: summary, detail: detail, life: life });
  }

  error(detail: any, summary: any = 'Error', life: number = this.lifetime) {
    console.error(detail);
    if (detail instanceof HttpErrorResponse) {
      summary = `Error: ${detail.status} ${detail.statusText}`;
      detail = `${detail.name} \r\n ${detail.message}`;
    }

    this.messageService.add({ severity: 'error', summary: summary, detail: detail.toString(), life: life });
  }

  contrast(detail: any, summary: any = 'Error', life: number = this.lifetime) {
    this.messageService.add({ severity: 'contrast', summary: summary, detail: detail, life: life });
  }

  secondary(detail: any, summary: any = 'Secondary', life: number = this.lifetime) {
    this.messageService.add({ severity: 'secondary', summary: summary, detail: detail, life: life });
  }
}

