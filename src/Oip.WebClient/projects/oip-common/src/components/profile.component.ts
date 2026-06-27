import { Component, inject } from '@angular/core';
import { FileUploadModule } from 'primeng/fileupload';
import { ImageModule } from 'primeng/image';
import { AvatarModule } from 'primeng/avatar';
import { MsgService } from '../services/msg.service';
import { UserService } from '../services/user.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserProfileApi } from '../api/user-profile.api';

@Component({
  selector: 'user-profile',
  standalone: true,
  imports: [FileUploadModule, ImageModule, AvatarModule, TranslatePipe],
  template: `
    <p-avatar
      class="mr-2"
      id="oip-user-profile-photo-avatar"
      shape="circle"
      size="xlarge"
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
        [customUpload]="true"
        [auto]="true"
        (uploadHandler)="uploadPhoto($event)" />
    </div>
  `
})
export class ProfileComponent {
  readonly userService = inject(UserService);
  readonly msgService = inject(MsgService);
  readonly translateService = inject(TranslateService);
  readonly userProfileApi = inject(UserProfileApi);

  async uploadPhoto(event: any): Promise<void> {
    const file = event.files?.[0];
    if (!file) {
      return;
    }

    try {
      await this.userProfileApi.postUserPhoto({ files: file });
      this.userService.refreshUserPhoto();
      this.msgService.success(this.translateService.instant('profileComponent.successfullyUploaded'));
    } catch (error) {
      this.msgService.error(error);
    }
  }
}
