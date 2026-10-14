import { Component, inject, ViewChild } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { MsgService } from '../../services/msg.service';
import { UserService } from '../../services/user.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserProfileApi } from '../../api/user-profile.api';
import { provideTranslations } from '../../helpers/l10n.helper';
import { UserPhotoCropperComponent } from './user-photo-cropper.component';

import en from './l10n/profile.en.json';
import ru from './l10n/profile.ru.json';

/** Largest source image the user may choose; the uploaded photo itself is always a 256×256 crop. */
const MAX_SOURCE_FILE_SIZE = 10 * 1024 * 1024;

@Component({
  selector: 'user-profile',
  standalone: true,
  imports: [ButtonModule, ConfirmDialog, DialogModule, TranslatePipe, UserPhotoCropperComponent],
  providers: [ConfirmationService],
  template: `
    <p-confirmDialog></p-confirmDialog>
    <div class="relative w-32 h-32" id="oip-user-profile-photo-avatar">
      <button
        class="group relative w-full h-full rounded-full overflow-hidden flex items-center justify-center bg-surface-200 dark:bg-surface-700 text-surface-700 dark:text-surface-0 cursor-pointer focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
        type="button"
        [attr.aria-label]="'profileComponent.changePhoto' | translate"
        [disabled]="uploading"
        (click)="fileInput.click()">
        @if (userService.photoLoaded) {
          <img alt="" class="w-full h-full object-cover" [src]="userService.photo" />
        } @else {
          <span class="text-3xl font-semibold">{{ userService.shortLabel }}</span>
        }
        <span
          class="absolute inset-0 flex items-center justify-center rounded-full bg-black/50 text-white transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100"
          [class.opacity-0]="userService.photoLoaded && !uploading">
          <i
            class="pi !text-[2.5rem]"
            [class.pi-camera]="!uploading"
            [class.pi-spin]="uploading"
            [class.pi-spinner]="uploading"></i>
        </span>
      </button>
      @if (userService.photoLoaded) {
        <!-- 50% ± 50%·sin 45° puts the button on the circle at «13:30» -->
        <button
          class="absolute w-7 h-7 -translate-x-1/2 -translate-y-1/2 rounded-full flex items-center justify-center cursor-pointer bg-surface-0 dark:bg-surface-900 border border-surface shadow text-red-500 hover:bg-red-50 dark:hover:bg-surface-800"
          id="oip-user-profile-delete-photo"
          style="left: 85.355%; top: 14.645%"
          type="button"
          [attr.aria-label]="'profileComponent.deletePhoto' | translate"
          [disabled]="uploading"
          (click)="confirmDeletePhoto($event)">
          <i class="pi pi-times !text-xs"></i>
        </button>
      }
      <input
        #fileInput
        accept="image/*"
        class="hidden"
        id="oip-user-profile-file-input"
        type="file"
        (change)="onFileSelected($event)" />
    </div>

    <p-dialog
      [breakpoints]="{ '575px': '95vw' }"
      [draggable]="false"
      [header]="'profileComponent.cropHeader' | translate"
      [modal]="true"
      [resizable]="false"
      [style]="{ width: '30rem' }"
      [(visible)]="cropVisible"
      (onHide)="selectedFile = null">
      @if (selectedFile) {
        <user-photo-cropper [file]="selectedFile" (cropped)="uploadPhoto($event)" (loadFailed)="onCropLoadFailed()" />
      }
      <ng-template #footer>
        <p-button
          id="oip-user-profile-crop-cancel"
          severity="secondary"
          [disabled]="uploading"
          [label]="'profileComponent.cancel' | translate"
          [outlined]="true"
          (onClick)="closeCrop()" />
        <p-button
          icon="pi pi-check"
          id="oip-user-profile-crop-save"
          [label]="'profileComponent.save' | translate"
          [loading]="uploading"
          (onClick)="cropper?.crop()" />
      </ng-template>
    </p-dialog>
  `
})
export class UserProfileComponent {
  private readonly translations = provideTranslations({ en, ru });

  readonly userService = inject(UserService);
  readonly msgService = inject(MsgService);
  readonly translateService = inject(TranslateService);
  readonly userProfileApi = inject(UserProfileApi);
  private readonly confirmationService = inject(ConfirmationService);

  @ViewChild(UserPhotoCropperComponent) cropper?: UserPhotoCropperComponent;

  cropVisible = false;
  selectedFile: File | null = null;
  uploading = false;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    // Reset the input so that choosing the same file again fires change once more.
    input.value = '';
    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.msgService.error(this.translateService.instant('profileComponent.invalidFile'));
      return;
    }

    if (file.size > MAX_SOURCE_FILE_SIZE) {
      this.msgService.error(this.translateService.instant('profileComponent.fileTooLarge'));
      return;
    }

    this.selectedFile = file;
    this.cropVisible = true;
  }

  onCropLoadFailed(): void {
    this.msgService.error(this.translateService.instant('profileComponent.invalidFile'));
    this.closeCrop();
  }

  closeCrop(): void {
    this.cropVisible = false;
    this.selectedFile = null;
  }

  async uploadPhoto(file: File): Promise<void> {
    if (this.uploading) {
      return;
    }

    this.uploading = true;
    try {
      await this.userProfileApi.postUserPhoto({ files: file });
      this.userService.refreshUserPhoto();
      this.msgService.success(this.translateService.instant('profileComponent.successfullyUploaded'));
      this.closeCrop();
    } catch (error) {
      this.msgService.error(error);
    } finally {
      this.uploading = false;
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
