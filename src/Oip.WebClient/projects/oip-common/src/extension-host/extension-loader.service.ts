import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ExtensionLoaderService {
  private readonly loadedScripts = new Map<string, Promise<void>>();

  loadScript(scriptUrl: string): Promise<void> {
    const existing = this.loadedScripts.get(scriptUrl);
    if (existing) {
      return existing;
    }

    const loadPromise = new Promise<void>((resolve, reject) => {
      const script = document.createElement('script');
      script.type = 'module';
      script.src = scriptUrl;
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => {
        this.loadedScripts.delete(scriptUrl);
        reject(new Error(`Extension script could not be loaded: ${scriptUrl}`));
      };

      document.head.appendChild(script);
    });

    this.loadedScripts.set(scriptUrl, loadPromise);
    return loadPromise;
  }
}
