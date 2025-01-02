import { Component } from '@angular/core';
import { ToastMessageOptions, MessageService } from 'primeng/api';
import { Toast } from 'primeng/toast';
import { ButtonDirective } from 'primeng/button';
import { MessagesModule } from 'primeng/messages';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';

@Component({
    templateUrl: './messagesdemo.component.html',
    providers: [MessageService],
    imports: [Toast, ButtonDirective, MessagesModule, InputText, Message]
})
export class MessagesDemoComponent {

  msgs: ToastMessageOptions[] = [];

  constructor(private service: MessageService) {
  }

  showInfoViaToast() {
    this.service.add({ key: 'tst', severity: 'info', summary: 'Info Message', detail: 'PrimeNG rocks' });
  }

  showWarnViaToast() {
    this.service.add({ key: 'tst', severity: 'warn', summary: 'Warn Message', detail: 'There are unsaved changes' });
  }

  showErrorViaToast() {
    this.service.add({ key: 'tst', severity: 'error', summary: 'Error Message', detail: 'Validation failed' });
  }

  showSuccessViaToast() {
    this.service.add({ key: 'tst', severity: 'success', summary: 'Success Message', detail: 'Message sent' });
  }

  showInfoViaMessages() {
    this.msgs = [];
    this.msgs.push({ severity: 'info', summary: 'Info Message', detail: 'PrimeNG rocks' });
  }

  showWarnViaMessages() {
    this.msgs = [];
    this.msgs.push({ severity: 'warn', summary: 'Warn Message', detail: 'There are unsaved changes' });
  }

  showErrorViaMessages() {
    this.msgs = [];
    this.msgs.push({ severity: 'error', summary: 'Error Message', detail: 'Validation failed' });
  }

  showSuccessViaMessages() {
    this.msgs = [];
    this.msgs.push({ severity: 'success', summary: 'Success Message', detail: 'Message sent' });
  }

}
