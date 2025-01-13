import { Component } from '@angular/core';
import {FileUploadModule} from "primeng/fileupload";

@Component({
  selector: 'user-profile',
  standalone: true,
  imports: [
    FileUploadModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {

}
