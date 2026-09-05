import { InjectionToken } from '@angular/core';
import type { PaletteDesignToken, Preset } from '@primeuix/themes/types';

/**
 * Defers loading of a theme preset until the theme is actually selected.
 *
 * Presets are large, so a preset that is not the application default should be provided as a loader
 * built on a dynamic import, which keeps it out of the initial bundle:
 *
 * ```ts
 * { id: 'Lara', preset: () => import('@primeng/themes/lara').then((m) => m.default as Preset) }
 * ```
 */
export type AppThemePresetLoader = () => Promise<Preset>;

export interface AppThemePreset {
  id: string;
  label?: string;
  /** The preset itself, or a {@link AppThemePresetLoader} resolving it on first use. */
  preset: Preset | AppThemePresetLoader;
  primaryColors?: Record<string, PaletteDesignToken | undefined>;
  surfaceColors?: Record<string, PaletteDesignToken | undefined>;
}

export type AppThemePresetMergeMode = 'mergeWithDefaults' | 'replaceDefaults';

export const APP_THEME_PRESETS = new InjectionToken<ReadonlyArray<AppThemePreset>>('APP_THEME_PRESETS', {
  factory: () => []
});

export const APP_THEME_PRESETS_MERGE_MODE = new InjectionToken<AppThemePresetMergeMode>(
  'APP_THEME_PRESETS_MERGE_MODE',
  {
    factory: () => 'mergeWithDefaults'
  }
);
