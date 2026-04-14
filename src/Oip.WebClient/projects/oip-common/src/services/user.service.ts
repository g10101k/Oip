import { inject, Injectable } from '@angular/core';
import { SecurityService } from './security.service';
import { BaseDataService } from './base-data.service';
import { distinctUntilChanged, filter, map } from 'rxjs/operators';

/**
 * UserService is responsible for retrieving and handling user-related data,
 * including the user's photo and short label for avatar display.
 */
@Injectable()
export class UserService {
  private readonly securityService = inject(SecurityService);
  private readonly baseDataService = inject(BaseDataService);
  private requestedPhotoEmail: string | null = null;

  constructor() {
    this.securityService
      .getCurrentUser$()
      .pipe(
        map((user) => user?.email ?? null),
        filter((email) => !!email),
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

    return `${givenNameInitial ?? ''}${familyNameInitial ?? ''}`.toUpperCase();
  }

  get userName(): string {
    const data = this.securityService.getCurrentUser();
    return [data?.given_name, data?.family_name].filter(Boolean).join(' ');
  }

  /**
   * Initiates an HTTP request to fetch the user's photo based on their email,
   * and updates the `photo` and `photoLoaded` properties accordingly.
   */
  getUserPhoto(): void {
    const email = this.securityService.getCurrentUser()?.email;

    if (!email) {
      return;
    }

    if (this.requestedPhotoEmail === email && (this.photoLoaded || this.photo)) {
      return;
    }

    this.requestedPhotoEmail = email;

    const url = this.baseDataService.buildUrl(
      `api/user-profile/get-user-photo?email=${email}`
    );
    this.baseDataService.getBlob(url).then(
      (data) => {
        this.createImageFromBlob(data as Blob);
        this.photoLoaded = true;
      },
      (error) => {
        console.log(error);
        this.photoLoaded = false;
      }
    );
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
}
