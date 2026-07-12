const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      display: block;
      min-height: 100%;
      color: var(--text-color, #0f172a);
      font-family: var(--font-family, Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif);
    }

    .shell {
      display: grid;
      gap: 1rem;
      padding: 1rem;
    }

    .hero,
    .panel {
      border: 1px solid color-mix(in srgb, currentColor 12%, transparent);
      border-radius: 8px;
      background: var(--surface-card, #ffffff);
      box-shadow: 0 12px 36px color-mix(in srgb, #0f172a 8%, transparent);
    }

    .hero {
      display: grid;
      gap: .75rem;
      padding: 1.25rem;
    }

    .eyebrow {
      color: #059669;
      font-size: .8rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: .04em;
    }

    h2 {
      margin: 0;
      font-size: 1.35rem;
      line-height: 1.25;
    }

    p {
      margin: 0;
      color: var(--text-color-secondary, #475569);
      line-height: 1.55;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1rem;
    }

    .panel {
      display: grid;
      gap: .5rem;
      padding: 1rem;
    }

    .label {
      color: var(--text-color-secondary, #64748b);
      font-size: .8rem;
    }

    .value {
      overflow-wrap: anywhere;
      font-weight: 650;
    }

    .actions {
      display: flex;
      flex-wrap: wrap;
      gap: .5rem;
    }

    button {
      min-height: 2.5rem;
      border: 0;
      border-radius: 6px;
      padding: .55rem .85rem;
      background: #059669;
      color: white;
      cursor: pointer;
      font: inherit;
      font-weight: 650;
    }

    button.secondary {
      background: #334155;
    }

    pre {
      overflow: auto;
      margin: 0;
      border-radius: 6px;
      padding: .75rem;
      background: #0f172a;
      color: #e2e8f0;
      font-size: .85rem;
      line-height: 1.45;
    }
  </style>
  <section class="shell">
    <div class="hero">
      <div class="eyebrow">Web Component extension</div>
      <h2>OIP Module Example</h2>
      <p>
        This module is loaded from Oip.ModuleExample at runtime. It receives OIP context,
        talks to its backend through the OIP proxy, and emits host events.
      </p>
      <div class="actions">
        <button data-action="load">Load backend status</button>
        <button class="secondary" data-action="notify">Send notification</button>
        <button class="secondary" data-action="settings">Save demo settings</button>
      </div>
    </div>
    <div class="grid">
      <div class="panel">
        <div class="label">Extension key</div>
        <div class="value" data-field="extensionKey">-</div>
      </div>
      <div class="panel">
        <div class="label">Module instance</div>
        <div class="value" data-field="moduleInstanceId">-</div>
      </div>
      <div class="panel">
        <div class="label">Permissions</div>
        <div class="value" data-field="permissions">-</div>
      </div>
    </div>
    <div class="panel">
      <div class="label">Backend response</div>
      <pre data-field="status">Click "Load backend status".</pre>
    </div>
    <div class="panel">
      <div class="label">Host context</div>
      <pre data-field="context">{}</pre>
    </div>
  </section>
`;

class OipModuleExampleElement extends HTMLElement {
  #context = null;
  #root = this.attachShadow({ mode: 'open' });

  constructor() {
    super();
    this.#root.appendChild(template.content.cloneNode(true));
  }

  connectedCallback() {
    this.#root.querySelector('[data-action="load"]').addEventListener('click', () => this.#loadStatus());
    this.#root.querySelector('[data-action="notify"]').addEventListener('click', () => this.#notify());
    this.#root.querySelector('[data-action="settings"]').addEventListener('click', () => this.#saveDemoSettings());
    this.dispatchEvent(new CustomEvent('oip:title-change', {
      detail: 'OIP Module Example',
      bubbles: true
    }));
    this.#render();
  }

  set oipContext(value) {
    this.#context = value;
    this.#render();
  }

  get oipContext() {
    return this.#context;
  }

  async #loadStatus() {
    if (!this.#context?.apiBasePath) {
      this.#setStatus({ error: 'OIP context has no apiBasePath yet.' });
      return;
    }

    try {
      const response = await fetch(`${this.#context.apiBasePath}/status`, {
        credentials: 'include'
      });
      const payload = await response.json();
      this.#setStatus(payload);
    } catch (error) {
      this.#setStatus({ error: error instanceof Error ? error.message : String(error) });
      this.dispatchEvent(new CustomEvent('oip:error', { detail: error, bubbles: true }));
    }
  }

  #notify() {
    this.dispatchEvent(new CustomEvent('oip:notify', {
      detail: {
        severity: 'success',
        summary: 'OIP Module Example',
        detail: 'The extension event reached the OIP host.'
      },
      bubbles: true
    }));
  }

  #saveDemoSettings() {
    this.dispatchEvent(new CustomEvent('oip:settings-change', {
      detail: {
        accent: 'emerald',
        savedAt: new Date().toISOString()
      },
      bubbles: true
    }));
  }

  #render() {
    const context = this.#context ?? {};
    this.#setText('extensionKey', context.extensionKey ?? '-');
    this.#setText('moduleInstanceId', context.moduleInstanceId ?? '-');
    this.#setText('permissions', JSON.stringify(context.permissions ?? {}, null, 2));
    this.#setText('context', JSON.stringify({
      moduleInstanceId: context.moduleInstanceId,
      extensionKey: context.extensionKey,
      apiBasePath: context.apiBasePath,
      locale: context.locale,
      permissions: context.permissions,
      settings: context.settings
    }, null, 2));
  }

  #setStatus(value) {
    this.#setText('status', JSON.stringify(value, null, 2));
  }

  #setText(field, value) {
    const element = this.#root.querySelector(`[data-field="${field}"]`);
    if (element) {
      element.textContent = String(value);
    }
  }
}

if (!customElements.get('oip-module-example')) {
  customElements.define('oip-module-example', OipModuleExampleElement);
}
