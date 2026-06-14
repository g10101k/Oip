import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FrontendRemoteManifestDto } from 'oip-common';

@Component({
  selector: 'app-remote-load-error',
  standalone: true,
  template: `
    <div class="flex min-h-96 items-center justify-center p-6">
      <div class="w-full max-w-2xl rounded border border-red-200 bg-red-50 p-6 text-red-900">
        <h1 class="mb-3 text-2xl font-semibold">Module is unavailable</h1>
        <p class="mb-4 text-base">
          {{ manifest?.title || manifest?.code || 'Remote module' }} cannot be loaded right now.
        </p>
        <p class="rounded bg-white/70 p-3 text-sm">{{ errorMessage }}</p>
      </div>
    </div>
  `
})
export class RemoteLoadErrorComponent {
  private readonly route = inject(ActivatedRoute);

  protected readonly manifest = this.route.snapshot.data['remoteManifest'] as FrontendRemoteManifestDto | undefined;
  protected readonly errorMessage = this.route.snapshot.data['remoteLoadError'] ?? 'Unexpected remote module error.';
}
