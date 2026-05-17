import { inject, Injectable } from '@angular/core';
import { ApplicationsApi } from '../api/applications.api';
import { ApplicationRegistryItemDto } from '../api/data-contracts'

@Injectable()
export class ApplicationRegistryService {
  private readonly applicationsApi = inject(ApplicationsApi);

  applications: ApplicationRegistryItemDto[] = [];
  currentApplication: ApplicationRegistryItemDto | null = null;

  async loadApplications(): Promise<void> {
    this.applications = await this.applicationsApi.get();
    this.currentApplication = this.applications.find((application) => application.isCurrent) ?? null;
  }

  openApplication(application: ApplicationRegistryItemDto): void {
    if (application.isCurrent || !application.baseUrl) {
      return;
    }

    window.location.href = application.baseUrl;
  }
}
