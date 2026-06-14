import { LocationStrategy, PathLocationStrategy } from '@angular/common';
import { HttpClient, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { importProvidersFrom, makeEnvironmentProviders } from '@angular/core';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { MessageService } from 'primeng/api';
import { NotificationApi } from '../api/notification.api';
import { SecurityApi } from '../api/security.api';
import { UserProfileApi } from '../api/user-profile.api';
import { langIntercept } from '../intercepts/i18n-intercept.service';
import { AuthGuardService } from '../services/auth-guard.service';
import { NotificationService } from '../services/notification.service';
import { BffSecurityService, SecurityService } from '../services/security.service';
import { UserService } from '../services/user.service';

const httpLoaderFactory: (http: HttpClient) => TranslateHttpLoader = (http: HttpClient) =>
  new TranslateHttpLoader(http);

export function provideOip() {
  return makeEnvironmentProviders([
    provideHttpClient(withInterceptors([langIntercept]), withFetch()),
    { provide: LocationStrategy, useClass: PathLocationStrategy },
    { provide: SecurityService, useClass: BffSecurityService },
    AuthGuardService,
    MessageService,
    UserService,
    NotificationService,
    UserProfileApi,
    SecurityApi,
    NotificationApi,
    importProvidersFrom([
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: httpLoaderFactory,
          deps: [HttpClient]
        }
      })
    ])
  ]);
}
