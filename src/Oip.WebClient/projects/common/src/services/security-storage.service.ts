import { AbstractSecurityStorage } from 'angular-auth-oidc-client';
import { Injectable } from "@angular/core";

@Injectable()
export class SecurityStorageService implements AbstractSecurityStorage {
  read(key: string) {
    return localStorage.getItem(key);
  }

  write(key: string, value: any): void {
    localStorage.setItem(key, value);
  }

  remove(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }
}
