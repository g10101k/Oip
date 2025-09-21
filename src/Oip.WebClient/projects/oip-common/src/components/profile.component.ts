import { Component, inject } from '@angular/core';
import { FileUploadModule } from 'primeng/fileupload';
import { ImageModule } from 'primeng/image';
import { AvatarModule } from 'primeng/avatar';
import { MsgService, UserService } from 'oip-common';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'user-profile',
  standalone: true,
  imports: [FileUploadModule, ImageModule, ImageModule, FileUploadModule, ImageModule, AvatarModule, TranslatePipe],
  template: `
    <p-avatar
      id="oip-user-profile-photo-avatar"
      shape="circle"
      size="xlarge"
      styleClass="mr-2"
      [image]="userService.photoLoaded ? userService.photo : null" />
    <div class="mt-2">
      <p-fileupload
        accept="image/*"
        chooseIcon="pi pi-upload"
        chooseLabel="{{ 'profileComponent.changePhoto' | translate }}"
        id="oip-user-profile-file-upload"
        maxFileSize="1000000"
        mode="basic"
        name="files"
        url="/api/user-profile/post-user-photo"
        withCredentials="true"
        [auto]="true"
        (onUpload)="onBasicUploadAuto($event)" />
    </div>
  `
})
export class ProfileComponent {
  readonly userService = inject(UserService);
  readonly msgService = inject(MsgService);
  readonly translateService = inject(TranslateService);

  onBasicUploadAuto($event) {
    this.msgService.success(this.translateService.instant('profileComponent.successfullyUploaded'));
  }
}
