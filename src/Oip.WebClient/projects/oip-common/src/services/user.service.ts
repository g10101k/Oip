import { inject, Injectable } from '@angular/core';
import { SecurityService } from "./security.service";
import { BaseDataService } from "./base-data.service";

@Injectable()
export class UserService {
  private readonly securityService = inject(SecurityService);
  private readonly baseDataService = inject(BaseDataService);

  constructor() {
    this.getUserPhoto();
  }

  /**
   * User photo
   */
  photo: any = null;

  /*
  * Photo is loaded
  * */
  photoLoaded: boolean = false;

  /*
  * Short label for avatar
  * */
  get shortLabel(): string {
    let data = this.securityService.userData;
    return data.given_name[0] + data.family_name[0];
  }

  /*
  * Get user photo
  * */
  getUserPhoto() {
    let url = `/api/user-profile/get-user-photo?email=${this.securityService.userData.email}`;
    this.baseDataService.getBlob(url)
      .then(data => {
        this.createImageFromBlob(data as Blob);
        this.photoLoaded = true;
      }, error => {
        console.log(error);
      });
  }

  private createImageFromBlob(image: Blob) {
    let reader = new FileReader();
    reader.addEventListener("load", () => {
      this.photo = reader.result;
    }, false);
    if (image) {
      reader.readAsDataURL(image);
    }
  }
}
