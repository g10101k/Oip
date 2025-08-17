import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { OpenIdConfiguration } from '../config/openid-configuration';
import { CallbackContext } from '../flows/callback-context';
import { FlowsService } from '../flows/flows.service';
import { ResetAuthDataService } from '../flows/reset-auth-data.service';
import { LoggerService } from '../logging/logger.service';
import { IntervalService } from './interval.service';

@Injectable({ providedIn: 'root' })
export class RefreshSessionRefreshTokenService {
  private readonly loggerService = inject(LoggerService);
  private readonly resetAuthDataService = inject(ResetAuthDataService);
  private readonly flowsService = inject(FlowsService);
  private readonly intervalService = inject(IntervalService);

  async refreshSessionWithRefreshTokens(
    config: OpenIdConfiguration,
    allConfigs: OpenIdConfiguration[],
    customParamsRefresh?: { [key: string]: string | number | boolean }
  ): Promise<CallbackContext> {
    this.loggerService.logDebug(config, 'BEGIN refresh session Authorize');
    let refreshTokenFailed = false;

    try {
      return this.flowsService
        .processRefreshToken(config, allConfigs, customParamsRefresh);
    }
    catch (error) {
      this.resetAuthDataService.resetAuthorizationData(config, allConfigs);
      refreshTokenFailed = true;
      throw new Error(error)
    }
    finally {
      refreshTokenFailed && this.intervalService.stopPeriodicTokenCheck()
    }
  }
}
