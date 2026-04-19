import { inject, Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastMessageOptions } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
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
    const validationMessage = this.extractValidationMessage(detail);
    if (validationMessage) {
      detail = validationMessage;
    } else if (detail instanceof HttpErrorResponse) {
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

  extractErrorMessage(error: unknown, fallback: string): string {
    const validationMessage = this.extractValidationMessage(error);
    if (validationMessage) {
      return validationMessage;
    }

    if (
      typeof error === 'object' &&
      error &&
      'error' in error &&
      typeof error.error === 'object' &&
      error.error &&
      'message' in error.error &&
      typeof error.error.message === 'string'
    ) {
      return error.error.message;
    }

    if (typeof error === 'object' && error && 'message' in error && typeof error.message === 'string') {
      return error.message;
    }

    return fallback;
  }

  private extractValidationMessage(error: unknown): string | null {
    const responseError = this.getObjectProperty(error, 'error');
    const validationErrors = this.getObjectProperty(responseError, 'errors') ?? this.getObjectProperty(error, 'errors');

    if (!validationErrors) {
      return null;
    }

    const messages = Object.entries(validationErrors)
      .reduce<string[]>((result, [field, value]) => [...result, ...this.toValidationMessages(field, value)], [])
      .filter((message) => message.length > 0);

    return messages.length > 0 ? messages.join('\n') : null;
  }

  private toValidationMessages(field: string, value: unknown): string[] {
    if (Array.isArray(value)) {
      return value
        .filter((message): message is string => typeof message === 'string')
        .map((message) => `${field}: ${message}`);
    }

    if (typeof value === 'string') {
      return [`${field}: ${value}`];
    }

    return [];
  }

  private getObjectProperty(source: unknown, property: string): Record<string, unknown> | null {
    if (typeof source !== 'object' || source === null || !(property in source)) {
      return null;
    }

    const value = (source as Record<string, unknown>)[property];
    return typeof value === 'object' && value !== null ? (value as Record<string, unknown>) : null;
  }

  errorFromException(error: unknown, fallback: string, summary: string = fallback, life: number = this.lifetime) {
    this.error(this.extractErrorMessage(error, fallback), summary, life);
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
