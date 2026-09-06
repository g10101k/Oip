import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, take } from 'rxjs/operators';
import { ButtonModule } from 'primeng/button';
import { TranslatePipe } from '@ngx-translate/core';
import { SecurityApi } from '../../api/security.api';
import { DefaultSecretFindingResponse, DefaultSecretsReportResponse } from '../../api/data-contracts';
import { SecurityService } from '../../services/security.service';
import { provideTranslations } from '../../helpers/l10n.helper';
import en from './l10n/default-secrets-banner.en.json';
import ru from './l10n/default-secrets-banner.ru.json';

/**
 * Administrator banner listing the secret settings that still hold a value shipped with the repository.
 *
 * The report comes from the same cached validator that feeds the startup log and the `default-secrets`
 * health check, so the three never disagree. Only administrators see it - the endpoint is admin-only.
 *
 * Whether the banner is raised at all is decided by the server through `showBanner`: findings are reported in
 * every environment, but the banner appears only in Production. Set `ASPNETCORE_ENVIRONMENT=Production` to see
 * it locally.
 */
@Component({
  selector: 'default-secrets-banner',
  standalone: true,
  imports: [ButtonModule, TranslatePipe],
  template: `
    @if (report(); as data) {
      <div
        class="mb-4 rounded-border border border-orange-300 bg-orange-50 p-4 dark:border-orange-700 dark:bg-orange-950">
        <div class="flex flex-row items-start gap-3">
          <i class="pi pi-exclamation-triangle mt-1 text-xl text-orange-600 dark:text-orange-400"></i>
          <div class="flex flex-col gap-1">
            <span class="font-bold text-surface-900 dark:text-surface-0">
              {{ 'defaultSecretsBanner.title' | translate }}
            </span>
            <span class="text-surface-700 dark:text-surface-200">
              {{
                'defaultSecretsBanner.description'
                  | translate: { count: data.findings?.length ?? 0, checked: data.checkedCount ?? 0 }
              }}
            </span>
          </div>
          <div class="ml-auto flex flex-row gap-2">
            <p-button
              severity="secondary"
              size="small"
              [icon]="expanded() ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
              [label]="
                (expanded() ? 'defaultSecretsBanner.actions.hide' : 'defaultSecretsBanner.actions.details') | translate
              "
              [text]="true"
              (click)="expanded.set(!expanded())" />
            <p-button
              icon="pi pi-times"
              severity="secondary"
              size="small"
              [ariaLabel]="'defaultSecretsBanner.actions.dismiss' | translate"
              [text]="true"
              (click)="dismiss()" />
          </div>
        </div>

        @if (expanded()) {
          <div class="mt-3 overflow-x-auto">
            <table class="w-full text-left text-sm">
              <thead>
                <tr class="text-surface-600 dark:text-surface-300">
                  <th class="py-1 pr-4" scope="col">{{ 'defaultSecretsBanner.columns.configKey' | translate }}</th>
                  <th class="py-1 pr-4" scope="col">{{ 'defaultSecretsBanner.columns.kind' | translate }}</th>
                  <th class="py-1" scope="col">
                    {{ 'defaultSecretsBanner.columns.environmentVariable' | translate }}
                  </th>
                </tr>
              </thead>
              <tbody>
                @for (finding of data.findings; track finding.configKey) {
                  <tr class="text-surface-900 dark:text-surface-0">
                    <td class="py-1 pr-4 font-mono">{{ finding.configKey }}</td>
                    <td class="py-1 pr-4">{{ kindLabel(finding) | translate }}</td>
                    <td class="py-1 font-mono">{{ finding.environmentVariable }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    }
  `
})
export class DefaultSecretsBannerComponent implements OnInit {
  private readonly translations = provideTranslations({ en, ru });
  private readonly securityApi = inject(SecurityApi);
  private readonly securityService = inject(SecurityService);

  private readonly destroyRef = inject(DestroyRef);

  protected readonly report = signal<DefaultSecretsReportResponse | null>(null);
  protected readonly expanded = signal(false);

  ngOnInit(): void {
    // `payload` emits on every auth() call, and the auth guard calls it on every route activation. The report
    // cannot change while the page is open, so wait for the first payload that belongs to an administrator and
    // request it exactly once. A non-administrator never issues the request at all - it would only earn a 403.
    this.securityService.payload
      .pipe(
        filter(() => this.securityService.isAdmin()),
        take(1),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.load());
  }

  protected kindLabel(finding: DefaultSecretFindingResponse): string {
    return `defaultSecretsBanner.kind.${finding.kind}`;
  }

  protected dismiss(): void {
    this.report.set(null);
  }

  private load(): void {
    this.securityApi
      .getDefaultSecretsReport()
      .then((data) => this.report.set(data?.showBanner ? data : null))
      // A service without the endpoint, or a revoked admin role, must not break the layout.
      .catch(() => this.report.set(null));
  }
}
