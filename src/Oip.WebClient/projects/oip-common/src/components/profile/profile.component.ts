import { Component, inject } from '@angular/core';
import { FileUploadModule } from "primeng/fileupload";
import { ImageModule } from "primeng/image";
import { AvatarModule } from "primeng/avatar";
import { UserService } from "../../services/user.service";

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
  templateUrl: './profile.component.html',
})
export class ProfileComponent  {
  userService = inject(UserService);
}
