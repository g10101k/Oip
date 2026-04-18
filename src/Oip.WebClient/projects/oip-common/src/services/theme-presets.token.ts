import { InjectionToken } from '@angular/core';
import type { PaletteDesignToken, Preset } from '@primeuix/themes/types';

export interface AppThemePreset {
  id: string;
  label?: string;
  preset: Preset;
  primaryColors?: Record<string, PaletteDesignToken | undefined>;
}

export type AppThemePresetMergeMode = 'mergeWithDefaults' | 'replaceDefaults';

export const APP_THEME_PRESETS = new InjectionToken<ReadonlyArray<AppThemePreset>>('APP_THEME_PRESETS', {
  factory: () => []
});

export const APP_THEME_PRESETS_MERGE_MODE = new InjectionToken<AppThemePresetMergeMode>('APP_THEME_PRESETS_MERGE_MODE', {
  factory: () => 'mergeWithDefaults'
});
