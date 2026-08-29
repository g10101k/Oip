import { Component, inject } from '@angular/core';
import { FileUploadModule } from 'primeng/fileupload';
import { ImageModule } from 'primeng/image';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { MsgService } from '../../services/msg.service';
import { UserService } from '../../services/user.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserProfileApi } from '../../api/user-profile.api';
import { provideTranslations } from '../../helpers/l10n.helper';

import en from './l10n/profile.en.json';
import ru from './l10n/profile.ru.json';

@Component({
  selector: 'user-profile',
  standalone: true,
  imports: [FileUploadModule, ImageModule, AvatarModule, ButtonModule, ConfirmDialog, TranslatePipe],
  providers: [ConfirmationService],
  template: `
    <p-confirmDialog></p-confirmDialog>
    <p-avatar
      class="mr-2"
      id="oip-user-profile-photo-avatar"
      shape="circle"
      size="xlarge"
      [image]="userService.photoLoaded ? userService.photo : null" />
    <div class="mt-2 flex gap-2">
      <p-fileupload
        accept="image/*"
        chooseIcon="pi pi-upload"
        chooseLabel="{{ 'profileComponent.changePhoto' | translate }}"
        id="oip-user-profile-file-upload"
        maxFileSize="1000000"
        mode="basic"
        name="files"
        [auto]="true"
        [customUpload]="true"
        (uploadHandler)="uploadPhoto($event)" />
      @if (userService.photoLoaded) {
        <p-button
          icon="pi pi-trash"
          id="oip-user-profile-delete-photo"
          label="{{ 'profileComponent.deletePhoto' | translate }}"
          severity="danger"
          [outlined]="true"
          (onClick)="confirmDeletePhoto($event)" />
      }
    </div>
  `
})
export class UserProfileComponent {
  private readonly translations = provideTranslations({ en, ru });

  readonly userService = inject(UserService);
  readonly msgService = inject(MsgService);
  readonly translateService = inject(TranslateService);
  readonly userProfileApi = inject(UserProfileApi);
  private readonly confirmationService = inject(ConfirmationService);

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

  confirmDeletePhoto(event: Event): void {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      header: this.translateService.instant('profileComponent.deletePhotoConfirmHeader'),
      message: this.translateService.instant('profileComponent.deletePhotoConfirmMessage'),
      icon: 'pi pi-trash',
      rejectButtonProps: {
        label: this.translateService.instant('profileComponent.deletePhotoConfirmRejectButtonPropsLabel'),
        severity: 'secondary',
        outlined: true
      },
      acceptButtonProps: {
        label: this.translateService.instant('profileComponent.deletePhotoConfirmAcceptButtonPropsLabel'),
        severity: 'danger'
      },
      accept: async () => {
        await this.deletePhoto();
      }
    });
  }

  private async deletePhoto(): Promise<void> {
    try {
      await this.userProfileApi.deleteUserPhoto();
      this.userService.refreshUserPhoto();
      this.msgService.success(this.translateService.instant('profileComponent.photoDeleted'));
    } catch (error) {
      this.msgService.errorFromException(error, this.translateService.instant('profileComponent.failedToDeletePhoto'));
    }
  }
}
