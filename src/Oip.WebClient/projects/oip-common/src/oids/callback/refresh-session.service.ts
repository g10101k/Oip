import { inject, Injectable } from '@angular/core';
import {
  forkJoin, from, lastValueFrom,
  Observable,
  of,
  throwError,
  TimeoutError,
  timer,
} from 'rxjs';
import {
  filter,
  map,
  mergeMap,
  retryWhen,
  switchMap,
  take,
  tap,
  timeout,
} from 'rxjs/operators';
import { AuthStateService } from '../auth-state/auth-state.service';
import { AuthWellKnownService } from '../config/auth-well-known/auth-well-known.service';
import { OpenIdConfiguration } from '../config/openid-configuration';
import { CallbackContext } from '../flows/callback-context';
import { FlowsDataService } from '../flows/flows-data.service';
import { RefreshSessionIframeService } from '../iframe/refresh-session-iframe.service';
import { SilentRenewService } from '../iframe/silent-renew.service';
import { LoggerService } from '../logging/logger.service';
import { LoginResponse } from '../login/login-response';
import { StoragePersistenceService } from '../storage/storage-persistence.service';
import { UserService } from '../user-data/user.service';
import { FlowHelper } from '../utils/flowHelper/flow-helper.service';
import { RefreshSessionRefreshTokenService } from './refresh-session-refresh-token.service';

export const MAX_RETRY_ATTEMPTS = 3;

@Injectable({ providedIn: 'root' })
export class RefreshSessionService {
  private readonly flowHelper = inject(FlowHelper);
  private readonly flowsDataService = inject(FlowsDataService);
  private readonly loggerService = inject(LoggerService);
  private readonly silentRenewService = inject(SilentRenewService);
  private readonly authStateService = inject(AuthStateService);
  private readonly authWellKnownService = inject(AuthWellKnownService);
  private readonly refreshSessionIframeService = inject(
    RefreshSessionIframeService
  );
  private readonly storagePersistenceService = inject(
    StoragePersistenceService
  );
  private readonly refreshSessionRefreshTokenService = inject(
    RefreshSessionRefreshTokenService
  );
  private readonly userService = inject(UserService);

  async userForceRefreshSession(
    config: OpenIdConfiguration | null,
    allConfigs: OpenIdConfiguration[],
    extraCustomParams?: { [key: string]: string | number | boolean }
  ): Promise<LoginResponse> {
    if (!config) {
      throw new Error('Please provide a configuration before setting up the module');
    }
    this.persistCustomParams(extraCustomParams, config);

    let loginResponsePromise = this.forceRefreshSession(config, allConfigs, extraCustomParams)

    this.flowsDataService.resetSilentRenewRunning(config)

    return loginResponsePromise;
  }

  async forceRefreshSession(
    config: OpenIdConfiguration,
    allConfigs: OpenIdConfiguration[],
    extraCustomParams?: { [key: string]: string | number | boolean }
  ): Promise<LoginResponse> {
    const { customParamsRefreshTokenRequest, configId } = config;
    const mergedParams = { ...customParamsRefreshTokenRequest, ...extraCustomParams, };

    if (this.flowHelper.isCurrentFlowCodeFlowWithRefreshTokens(config)) {
      await this.startRefreshSession(config, allConfigs, mergedParams);

      const isAuthenticated = this.authStateService.areAuthStorageTokensValid(config);

      if (isAuthenticated) {
        return {
          idToken: this.authStateService.getIdToken(config),
          accessToken: this.authStateService.getAccessToken(config),
          userData: this.userService.getUserDataFromStore(config),
          isAuthenticated,
          configId,
        } as LoginResponse;
      }

      return {
        isAuthenticated: false,
        errorMessage: '',
        userData: null,
        idToken: '',
        accessToken: '',
        configId,
      };
    }

    const { silentRenewTimeoutInSeconds } = config;
    const timeOutTime = (silentRenewTimeoutInSeconds ?? 0) * 1000;

    return await lastValueFrom(forkJoin([
      from(this.startRefreshSession(config, allConfigs, extraCustomParams)),
      this.silentRenewService.refreshSessionWithIFrameCompleted$.pipe(
        filter((result) => result?.configId === config.configId),
        take(1)
      ),
    ]).pipe(
      timeout(timeOutTime),
      retryWhen((errors) => {
        return errors.pipe(
          mergeMap((error, index) => {
            const scalingDuration = 1000;
            const currentAttempt = index + 1;

            if (!(error instanceof TimeoutError) || currentAttempt > MAX_RETRY_ATTEMPTS) {
              return throwError(() => new Error(error));
            }

            this.loggerService.logDebug(config, `forceRefreshSession timeout. Attempt #${currentAttempt}`);

            this.flowsDataService.resetSilentRenewRunning(config);

            return timer(currentAttempt * scalingDuration);
          })
        );
      }),
      map(([_, refreshCompleted]) => {
        const isAuthenticated = this.authStateService.areAuthStorageTokensValid(config);

        if (isAuthenticated) {
          const authResult = refreshCompleted.success ? refreshCompleted.authResult : null

          return {
            idToken: authResult?.id_token ?? '',
            accessToken: authResult?.access_token ?? '',
            userData: this.userService.getUserDataFromStore(config),
            isAuthenticated,
            configId,
          };
        }

        return {
          isAuthenticated: false,
          errorMessage: '',
          userData: null,
          idToken: '',
          accessToken: '',
          configId,
        };
      })
    ));
  }

  private persistCustomParams(
    extraCustomParams: { [key: string]: string | number | boolean } | undefined,
    config: OpenIdConfiguration
  ): void {
    const { useRefreshToken } = config;

    if (extraCustomParams) {
      if (useRefreshToken) {
        this.storagePersistenceService.write(
          'storageCustomParamsRefresh',
          extraCustomParams,
          config
        );
      } else {
        this.storagePersistenceService.write(
          'storageCustomParamsAuthRequest',
          extraCustomParams,
          config
        );
      }
    }
  }

  private async startRefreshSession(
    config: OpenIdConfiguration,
    allConfigs: OpenIdConfiguration[],
    extraCustomParams?: { [key: string]: string | number | boolean }
  ): Promise<boolean | CallbackContext | null> {
    const isSilentRenewRunning = this.flowsDataService.isSilentRenewRunning(config);

    this.loggerService.logDebug(config, `Checking: silentRenewRunning: ${isSilentRenewRunning}`);
    const shouldBeExecuted = !isSilentRenewRunning;

    if (!shouldBeExecuted) {
      return null;
    }
    await this.authWellKnownService.queryAndStoreAuthWellKnownEndPoints(config);

    this.flowsDataService.setSilentRenewRunning(config);

    if (this.flowHelper.isCurrentFlowCodeFlowWithRefreshTokens(config)) {
      // Refresh Session using Refresh tokens
      return this.refreshSessionRefreshTokenService.refreshSessionWithRefreshTokens(
        config,
        allConfigs,
        extraCustomParams
      );
    }

    return await lastValueFrom(this.refreshSessionIframeService.refreshSessionWithIframe(
      config,
      allConfigs,
      extraCustomParams
    ));
  }
}
