import {inject, Injectable} from '@angular/core';
import {SecurityService} from './security.service';
import {BaseDataService} from './base-data.service';

/**
 * UserService is responsible for retrieving and handling user-related data,
 * including the user's photo and short label for avatar display.
 */
@Injectable()
export class UserService {
  private readonly securityService = inject(SecurityService);
  private readonly baseDataService = inject(BaseDataService);

  constructor() {
    this.getUserPhoto();
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
    let data = this.securityService.userData;
    return data.given_name[0] + data.family_name[0];
  }

  get userName(): string {
    let data = this.securityService.userData;
    return `${data.given_name} ${data.family_name}`;
  }

  /**
   * Initiates an HTTP request to fetch the user's photo based on their email,
   * and updates the `photo` and `photoLoaded` properties accordingly.
   */
  getUserPhoto(): void {
    let url = `${this.baseDataService.baseUrl}api/user-profile/get-user-photo?email=${this.securityService.userData.email}`;
    this.baseDataService.getBlob(url)
      .then(data => {
        this.createImageFromBlob(data as Blob);
        this.photoLoaded = true;
      }, error => {
        console.log(error);
      });
  }

  /**
   * Converts a Blob image into a Base64 data URL and stores it in the `photo` property.
   *
   * @param image - The image Blob to be converted.
   */
  private createImageFromBlob(image: Blob): void {
    let reader = new FileReader();
    reader.addEventListener('load', () => {
      this.photo = reader.result;
    }, false);
    if (image) {
      reader.readAsDataURL(image);
    }
  }
}
