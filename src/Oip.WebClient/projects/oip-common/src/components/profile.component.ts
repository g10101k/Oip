import { Component, inject } from '@angular/core';
import { FileUploadModule } from "primeng/fileupload";
import { ImageModule } from "primeng/image";
import { AvatarModule } from "primeng/avatar";
import { UserService } from "oip-common";

@Component({
  selector: 'user-profile',
  standalone: true,
  imports: [
    FileUploadModule,
    ImageModule,
    ImageModule,
    FileUploadModule,
    ImageModule,
    AvatarModule
  ],
  template: `<p-avatar
  [image]="userService.photoLoaded ? userService.photo : null"
  styleClass="mr-2"
  size="xlarge"
  shape="circle"/>

<div class="mt-2">
  <p-fileUpload
    mode="basic"
    name="files"
    chooseIcon="pi pi-upload"
    url="/api/user-profile/post-user-photo"
    accept="image/*" maxFileSize="1000000"
    withCredentials="true"
    chooseLabel="Change photo"/>
</div>
`,
})
export class ProfileComponent  {
  userService = inject(UserService);
}
