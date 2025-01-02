import { Component } from '@angular/core';
import { MessageService, PrimeTemplate } from 'primeng/api';
import { FileUpload } from 'primeng/fileupload';
import { NgIf, NgFor } from '@angular/common';

@Component({
    templateUrl: './filedemo.component.html',
    providers: [MessageService],
    imports: [FileUpload, PrimeTemplate, NgIf, NgFor]
})
export class FileDemoComponent {

  uploadedFiles: any[] = [];

  constructor(private messageService: MessageService) {
  }

  onUpload(event: any) {
    for (const file of event.files) {
      this.uploadedFiles.push(file);
    }

    this.messageService.add({ severity: 'info', summary: 'Success', detail: 'File Uploaded' });
  }

  onBasicUpload() {
    this.messageService.add({ severity: 'info', summary: 'Success', detail: 'File Uploaded with Basic Mode' });
  }

}
