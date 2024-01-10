import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.component.html',
})
export class AuthCallbackComponent implements OnInit {

  error: boolean = true;

  constructor(private authService: AuthService, private router: Router, private route: ActivatedRoute) {
  }

  async ngOnInit() {
    // check for error
    if (this.route?.snapshot?.fragment?.indexOf('error') >= 0) {
      this.error = true;
      return;
    }
    alert(this.route);
    await this.authService.completeAuthentication();
    this.router.navigate(['/home']);
  }
}
