import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { EventTypes } from '../../public-events/event-types';
import { PublicEventsService } from '../../public-events/public-events.service';
import { StoragePersistenceService } from '../../storage/storage-persistence.service';
import { OpenIdConfiguration } from '../openid-configuration';
import { AuthWellKnownDataService } from './auth-well-known-data.service';
import { AuthWellKnownEndpoints } from './auth-well-known-endpoints';

@Injectable({ providedIn: 'root' })
export class AuthWellKnownService {
  private readonly dataService = inject(AuthWellKnownDataService);
  private readonly publicEventsService = inject(PublicEventsService);
  private readonly storagePersistenceService = inject(
    StoragePersistenceService
  );

  storeWellKnownEndpoints(
    config: OpenIdConfiguration,
    mappedWellKnownEndpoints: AuthWellKnownEndpoints
  ): void {
    this.storagePersistenceService.write(
      'authWellKnownEndPoints',
      mappedWellKnownEndpoints,
      config
    );
  }

  async queryAndStoreAuthWellKnownEndPoints(config: OpenIdConfiguration | null): Promise<AuthWellKnownEndpoints> {
    if (!config) {
      throw new Error('Please provide a configuration before setting up the module');
    }

    try {
      let mappedWellKnownEndpoints = this.dataService.getWellKnownEndPointsForConfig(config);
      mappedWellKnownEndpoints.then(endpoints => {
        this.storeWellKnownEndpoints(config, endpoints)
      })
      return mappedWellKnownEndpoints;
    } catch (error) {
      this.publicEventsService.fireEvent(EventTypes.ConfigLoadingFailed, null);
      throw new Error(error);
    }
  }
}
