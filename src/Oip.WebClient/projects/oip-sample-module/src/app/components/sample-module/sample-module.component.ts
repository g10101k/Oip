import { HttpClient } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { combineLatest, of } from 'rxjs';
import { catchError, distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';

interface SampleModuleInfo {
  moduleInstanceId: number;
  name: string;
  serverTimeUtc: string;
}

@Component({
  selector: 'app-sample-module',
  standalone: true,
  imports: [ButtonModule, ProgressSpinnerModule, TagModule, TranslateModule],
  template: `
    <section class="sample-module">
      <div class="sample-module__header">
        <div>
          <h1>{{ 'sampleModule.title' | translate }}</h1>
          <p>{{ 'sampleModule.subtitle' | translate }}</p>
        </div>
        <p-tag value="Remote" severity="info" />
      </div>

      <div class="sample-module__panel">
        @if (loading()) {
          <p-progress-spinner ariaLabel="loading" />
        } @else if (error()) {
          <div class="sample-module__state">
            <strong>{{ 'sampleModule.errorTitle' | translate }}</strong>
            <span>{{ error() }}</span>
            <button pButton type="button" icon="pi pi-refresh" [label]="'sampleModule.retry' | translate" (click)="reload()"></button>
          </div>
        } @else if (info()) {
          <dl>
            <div>
              <dt>{{ 'sampleModule.instanceId' | translate }}</dt>
              <dd>{{ info()?.moduleInstanceId }}</dd>
            </div>
            <div>
              <dt>{{ 'sampleModule.moduleName' | translate }}</dt>
              <dd>{{ info()?.name }}</dd>
            </div>
            <div>
              <dt>{{ 'sampleModule.serverTime' | translate }}</dt>
              <dd>{{ info()?.serverTimeUtc }}</dd>
            </div>
          </dl>
        }
      </div>
    </section>
  `,
  styles: [
    `
      .sample-module {
        display: grid;
        gap: 1rem;
      }

      .sample-module__header,
      .sample-module__panel {
        background: var(--surface-card);
        border: 1px solid var(--surface-border);
        border-radius: 8px;
        padding: 1rem;
      }

      .sample-module__header {
        align-items: flex-start;
        display: flex;
        justify-content: space-between;
        gap: 1rem;
      }

      h1 {
        font-size: 1.25rem;
        margin: 0 0 0.25rem;
      }

      p {
        color: var(--text-color-secondary);
        margin: 0;
      }

      dl {
        display: grid;
        gap: 0.75rem;
        margin: 0;
      }

      dl > div {
        display: grid;
        gap: 0.25rem;
      }

      dt {
        color: var(--text-color-secondary);
        font-size: 0.875rem;
      }

      dd {
        margin: 0;
      }

      .sample-module__state {
        align-items: flex-start;
        display: grid;
        gap: 0.75rem;
      }
    `
  ]
})
export class SampleModuleComponent {
  private readonly httpClient = inject(HttpClient);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly info = signal<SampleModuleInfo | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  private currentId: number | null = null;

  constructor() {
    const currentParamMap$ = this.route.paramMap;
    const parentParamMap$ = this.route.parent?.paramMap ?? of(convertToParamMap({}));

    combineLatest([currentParamMap$, parentParamMap$])
      .pipe(
        map(([current, parent]) => current.get('id') ?? parent.get('id')),
        map((id) => (id == null ? null : Number(id))),
        distinctUntilChanged(),
        switchMap((id) => {
          this.currentId = id;
          return this.loadInfo(id);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  reload(): void {
    this.loadInfo(this.currentId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
  }

  private loadInfo(id: number | null) {
    this.loading.set(true);
    this.error.set(null);

    if (id == null || Number.isNaN(id)) {
      this.loading.set(false);
      this.error.set('Module instance id is missing.');
      return of(null);
    }

    return this.httpClient.get<SampleModuleInfo>(`/api/sample-module/${id}/info`).pipe(
      map((response) => {
        this.info.set(response);
        this.loading.set(false);
        return response;
      }),
      catchError((error) => {
        this.info.set(null);
        this.loading.set(false);
        this.error.set(error?.message ?? 'Unknown error.');
        return of(null);
      })
    );
  }
}
