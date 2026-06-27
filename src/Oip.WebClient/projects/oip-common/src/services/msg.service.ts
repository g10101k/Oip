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
    if (detail instanceof HttpErrorResponse) {
      summary = `Error: ${detail.status} ${detail.statusText}`;
      detail = this.extractErrorMessage(detail, `${detail.name} \r\n ${detail.message}`);
    } else {
      detail = this.extractErrorMessage(detail, detail?.toString?.() ?? this.translate.instant('msgService.error'));
    }

    this.messageService.add({
      severity: 'error',
      summary: summary,
      detail: detail,
      life: life
    });
  }

  extractErrorMessage(error: unknown, fallback: string): string {
    const validationMessage = this.extractValidationMessage(error);
    if (validationMessage) {
      return validationMessage;
    }

    const apiExceptionMessage = this.extractApiExceptionMessage(error);
    if (apiExceptionMessage) {
      return apiExceptionMessage;
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

  private extractApiExceptionMessage(error: unknown): string | null {
    const responseError = this.getObjectProperty(error, 'error');
    const message = this.getStringProperty(responseError, 'message') ?? this.getStringProperty(error, 'message');
    const title = this.getStringProperty(responseError, 'title') ?? this.getStringProperty(error, 'title');

    if (message && title) {
      return `${title}: ${message}`;
    }

    return message ?? title;
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

  private getStringProperty(source: unknown, property: string): string | null {
    if (typeof source !== 'object' || source === null || !(property in source)) {
      return null;
    }

    const value = (source as Record<string, unknown>)[property];
    return typeof value === 'string' && value.length > 0 ? value : null;
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
