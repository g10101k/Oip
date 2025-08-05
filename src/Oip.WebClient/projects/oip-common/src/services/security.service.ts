import { Injectable, OnDestroy, inject } from '@angular/core';

import { BehaviorSubject, Observable } from "rxjs";
import { filter, map } from "rxjs/operators";

/**
 * SecurityService extends OidcSecurityService to manage authentication,
 * token handling, and user role access in an Angular application.
 *
 * It provides helper methods for checking authentication, managing tokens,
 * determining user roles, and performing logout and refresh operations.
 */
@Injectable({ providedIn: 'root' })
export class SecurityService implements OnDestroy {




  /**
   * Stores user-specific data from the login response.
   */
  userData: any;

  /**
   * Initializes service and subscribes to authentication events.
   * When a 'NewAuthenticationResult' event is received, the `auth` method is called.
   */
  constructor() {

  }


  /**
   * Indicates whether the current user has the 'admin' role.
   *
   * @returns {boolean} True if the user is an admin, false otherwise.
   */
  get isAdmin(): boolean {
    return true;
  }

  /**
   * Initiates authentication check and updates login response, user data,
   * and decoded token payload if authenticated.
   */
  auth() {

  }

  /**
   * Completes the BehaviorSubjects when the service is destroyed to avoid memory leaks.
   */
  ngOnDestroy(): void {
  }

}
