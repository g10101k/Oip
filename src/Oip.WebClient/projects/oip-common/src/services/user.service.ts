import { inject, Injectable } from '@angular/core';
import { SecurityService } from './security.service';
import { distinctUntilChanged, filter, map } from 'rxjs/operators';
import { UserProfileApi } from '../api/user-profile.api';
import { MsgService } from './msg.service';
import { TranslateService } from '@ngx-translate/core';

/**
 * UserService is responsible for retrieving and handling user-related data,
 * including the user's photo and short label for avatar display.
 */
@Injectable()
export class UserService {
  private readonly securityService = inject(SecurityService);
  private readonly userProfileApi = inject(UserProfileApi);
  private readonly msgService = inject(MsgService);
  private readonly translateService = inject(TranslateService);
  private requestedPhotoKey: string | null = null;

  constructor() {
    this.securityService
      .getCurrentUser$()
      .pipe(
        map((user) => this.getCurrentUserPhotoKey(user)),
        filter((photoKey) => !!photoKey),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.getUserPhoto();
      });
  }

  /**
   * Stores the user's photo as a data URL or binary blob, depending on how it's processed.
   */
  photo: any = null;

  /**
   * Indicates whether the user photo has finished loading.
   */
  photoLoaded: boolean = false;

  /**
   * Returns a short label composed of the user's initials.
   * Typically used for avatar display when a photo is unavailable.
   */
  get shortLabel(): string {
    const data = this.securityService.getCurrentUser();
    const givenNameInitial = data?.given_name?.trim()?.[0];
    const familyNameInitial = data?.family_name?.trim()?.[0];
    const displayNameInitials = this.getInitials(data?.displayName ?? data?.name ?? data?.userName ?? data?.preferred_username);

    return `${givenNameInitial ?? ''}${familyNameInitial ?? ''}`.toUpperCase() || displayNameInitials;
  }

  get userName(): string {
    const data = this.securityService.getCurrentUser();
    return [data?.given_name, data?.family_name].filter(Boolean).join(' ')
      || data?.displayName
      || data?.name
      || data?.userName
      || data?.preferred_username
      || data?.email
      || '';
  }

  /**
   * Initiates an HTTP request to fetch the current user's photo,
   * and updates the `photo` and `photoLoaded` properties accordingly.
   */
  getUserPhoto(): void {
    const photoKey = this.getCurrentUserPhotoKey(this.securityService.getCurrentUser());

    if (!photoKey) {
      return;
    }

    if (this.requestedPhotoKey === photoKey && (this.photoLoaded || this.photo)) {
      return;
    }

    this.requestedPhotoKey = photoKey;

    this.userProfileApi.getUserPhoto({ format: 'blob' }).then(
      (data) => {
        this.createImageFromBlob(data as Blob);
        this.photoLoaded = true;
      },
      (error) => {
        this.msgService.errorFromException(error, this.translateService.instant('profileComponent.failedToLoadPhoto'));
        this.photoLoaded = false;
      }
    );
  }

  refreshUserPhoto(): void {
    this.requestedPhotoKey = null;
    this.photo = null;
    this.photoLoaded = false;
    this.getUserPhoto();
  }

  /**
   * Converts a Blob image into a Base64 data URL and stores it in the `photo` property.
   *
   * @param image - The image Blob to be converted.
   */
  private createImageFromBlob(image: Blob): void {
    const reader = new FileReader();
    reader.addEventListener(
      'load',
      () => {
        this.photo = reader.result;
      },
      false
    );
    if (image) {
      reader.readAsDataURL(image);
    }
  }

  private getInitials(value?: string): string {
    return value
      ?.trim()
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0])
      .join('')
      .toUpperCase() ?? '';
  }

  private getCurrentUserPhotoKey(user: any): string | null {
    return user?.sub
      || user?.userName
      || user?.preferred_username
      || user?.displayName
      || user?.name
      || null;
  }
}
